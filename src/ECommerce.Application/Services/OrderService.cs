using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    ICouponRepository couponRepository,
    IInventoryService inventoryService,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher,
    IMapper mapper
) : IOrderService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly ICouponRepository _couponRepository = couponRepository;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly IMapper _mapper = mapper;

    private async Task ReleaseInventoryForOrderAsync(Order order)
    {
        var reservations = order
            .OrderItems.Where(oi => oi.SkuId > 0 && oi.Quantity > 0)
            .Select(oi => (oi.SkuId, oi.Quantity))
            .ToList();

        if (reservations.Count == 0)
            return;

        await _inventoryService.ReleaseReservationAsync(
            order.OrderId,
            order.OrderNumber,
            reservations
        );
    }

    public async Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request)
    {
        // Get user's cart with details
        var cart =
            await _cartRepository.GetByUserIdWithDetailsAsync(userId)
            ?? throw new NotFoundException("Cart not found");

        var sellerIds = cart
            .CartItems.Select(i => i.Sku?.Product?.SellerId)
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (sellerIds.Count != 1)
            throw new BadRequestException(
                "Cart must contain items from a single seller to checkout"
            );

        if (cart.CartItems.Count == 0)
            throw new BadRequestException("Cart is empty");

        // Validate all items are still available
        foreach (var item in cart.CartItems)
        {
            var availableStock = item.Sku?.Inventory?.QuantityAvailable ?? 0;
            if (availableStock < item.Quantity)
            {
                throw new BadRequestException(
                    $"Insufficient stock for {item.Sku?.Product?.ProductName ?? "item"}. "
                        + $"Available: {availableStock}, Requested: {item.Quantity}"
                );
            }
        }

        // Calculate order totals
        var subtotal = cart.CartItems.Sum(i => i.PriceSnapshot * i.Quantity);
        var shippingFee = CalculateShippingFee(subtotal);
        var taxAmount = CalculateTax(subtotal);
        var (couponId, couponCode, couponDiscount) = await ResolveCouponAsync(
            request.CouponCode,
            subtotal,
            shippingFee,
            taxAmount
        );

        var totalBeforeDiscount = subtotal + shippingFee + taxAmount;
        var totalAmount = totalBeforeDiscount - couponDiscount;
        if (totalAmount < 0)
            totalAmount = 0;

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Create order
            var order = Order.CreateDefault(
                orderNumber: GenerateOrderNumber(),
                userId: userId,
                subtotal: subtotal,
                shippingFee: shippingFee,
                taxAmount: taxAmount,
                totalAmount: totalAmount,
                preferredCurrency: Currency.vnd,
                couponCode: couponCode,
                couponDiscount: couponDiscount,
                customerNotes: request.CustomerNotes
            );

            order.CouponId = couponId;

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = OrderItem.CreateDefault(
                    order: order,
                    skuId: cartItem.SkuId,
                    productName: cartItem.Sku?.Product?.ProductName ?? "Unknown",
                    sku: cartItem.Sku?.Sku ?? "Unknown",
                    sellerId: cartItem.Sku?.Product?.SellerId ?? 0,
                    quantity: cartItem.Quantity,
                    unitPrice: cartItem.PriceSnapshot,
                    variantDescription: string.Join(
                        ", ",
                        new[] { cartItem.Sku?.Color, cartItem.Sku?.Size }.Where(v =>
                            !string.IsNullOrWhiteSpace(v)
                        )
                    )
                );
                order.OrderItems.Add(orderItem);
            }

            var shippingAddress = await ResolveShippingAddressAsync(userId, request);
            // Create order shipping
            var orderShipping = OrderShipping.CreateDefault(
                order: order,
                recipientName: shippingAddress.RecipientName,
                phone: shippingAddress.Phone,
                addressLine1: shippingAddress.AddressLine1,
                city: shippingAddress.City,
                stateProvince: shippingAddress.StateProvince,
                postalCode: shippingAddress.PostalCode,
                country: shippingAddress.Country,
                method: ShippingMethod.standard,
                addressLine2: shippingAddress.AddressLine2
            );
            order.OrderShipping = orderShipping;

            await _unitOfWork.SaveChangesAsync();

            // Clear the cart after successful order
            foreach (var item in cart.CartItems.ToList())
            {
                await _cartRepository.RemoveCartItemAsync(item);
            }
            cart.CartItems.Clear();
            cart.RecalculateTotals();
            await _unitOfWork.SaveChangesAsync();

            // Publish order.created event
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                TaxAmount = order.TaxAmount,
                CouponCode = order.CouponCode,
                CouponDiscount = order.CouponDiscount,
                Items = order
                    .OrderItems.Select(i => new OrderItemEvent
                    {
                        SkuId = i.SkuId,
                        ProductName = i.ProductName,
                        Sku = i.Sku,
                        SellerId = i.SellerId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.Subtotal,
                    })
                    .ToList(),
            };

            await _eventPublisher.PublishAsync(orderCreatedEvent);

            await _unitOfWork.SaveChangesAsync();
            // Return the created order
            return _mapper.Map<OrderDto>(order);
        });
    }

    public async Task<OrderDto?> GetByIdAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);

        if (order == null || order.UserId != userId)
            return null;

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetUserOrdersAsync(
        int userId,
        PaginationParams paginationParams
    )
    {
        var (orders, totalCount) = await _orderRepository.GetOrdersByUserIdAsync(
            userId,
            paginationParams.PageNumber,
            paginationParams.PageSize
        );

        var orderSummaries = _mapper.Map<List<OrderSummaryDto>>(orders);

        return PagedResult<OrderSummaryDto>.Create(
            orderSummaries,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            totalCount
        );
    }

    public async Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(
        PaginationParams paginationParams
    )
    {
        var (orders, totalCount) = await _orderRepository.GetOrdersPagedAsync(
            paginationParams.PageNumber,
            paginationParams.PageSize
        );

        var orderSummaries = _mapper.Map<List<OrderSummaryDto>>(orders);

        return PagedResult<OrderSummaryDto>.Create(
            orderSummaries,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            totalCount
        );
    }

    public async Task<PagedResult<OrderSummaryDto>> GetSellerOrdersAsync(
        int sellerId,
        PaginationParams paginationParams
    )
    {
        var (orders, totalCount) = await _orderRepository.GetOrdersBySellerIdAsync(
            sellerId,
            paginationParams.PageNumber,
            paginationParams.PageSize
        );

        var orderSummaries = _mapper.Map<List<OrderSummaryDto>>(orders);

        return PagedResult<OrderSummaryDto>.Create(
            orderSummaries,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            totalCount
        );
    }

    public async Task<OrderDto?> GetSellerOrderByIdAsync(int sellerId, int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);

        if (order == null)
            return null;

        if (!order.OrderItems.Any(i => i.SellerId == sellerId))
            return null;

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetByIdAsAdminAsync(int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
        if (order == null)
            return null;
        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> CancelAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);

        if (order == null || order.UserId != userId)
            return null;

        // Buyer can only cancel before the seller has confirmed/processed the order.
        if (order.Status != OrderStatus.created)
            throw new BadRequestException(
                "Order can no longer be cancelled by buyer — please contact the seller"
            );

        order.Status = OrderStatus.cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Release reserved inventory back to "available"
        await ReleaseInventoryForOrderAsync(order);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> UpdateStatusAsSellerAsync(
        int sellerId,
        int orderId,
        UpdateOrderStatusRequest request
    )
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
        if (order == null)
            return null;

        if (!order.OrderItems.Any(i => i.SellerId == sellerId))
            throw new ForbiddenException("Not allowed to update this order");

        var distinctSellers = order.OrderItems.Select(i => i.SellerId).Distinct().ToList();
        if (distinctSellers.Count != 1 || distinctSellers[0] != sellerId)
            throw new BadRequestException("Order contains items from multiple sellers");

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            throw new BadRequestException("Invalid order status");

        if (
            newStatus
            is not (OrderStatus.confirmed or OrderStatus.delivered or OrderStatus.cancelled)
        )
        {
            throw new BadRequestException(
                "Seller can only set status to confirmed, delivered, or cancelled"
            );
        }

        var oldStatus = order.Status;
        if (oldStatus == newStatus)
            return _mapper.Map<OrderDto>(order);

        if (oldStatus is OrderStatus.delivered or OrderStatus.cancelled)
            throw new BadRequestException("Order is already finalized and cannot change status");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (newStatus == OrderStatus.cancelled)
        {
            order.CancelledAt = DateTime.UtcNow;
            // Seller-initiated cancel must also release reserved stock back to available.
            await ReleaseInventoryForOrderAsync(order);
        }

        order.OrderStatusHistories.Add(
            new OrderStatusHistory
            {
                OrderId = order.OrderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Notes = request.Notes,
                ChangedBy = sellerId,
                ChangedByNavigation = null!,
                Order = order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );

        await _unitOfWork.SaveChangesAsync();

        if (newStatus == OrderStatus.confirmed)
        {
            await _eventPublisher.PublishAsync(
                new OrderConfirmedEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                }
            );
        }

        if (newStatus == OrderStatus.shipped)
        {
            await _eventPublisher.PublishAsync(
                new OrderShippedEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                }
            );
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<OrderDto>(order);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{Guid.NewGuid():N}";
    }

    private static decimal CalculateShippingFee(decimal subtotal)
    {
        // Free shipping for orders over 500,000 VND
        return subtotal >= 500000 ? 0 : 30000;
    }

    private static decimal CalculateTax(decimal subtotal)
    {
        // 10% VAT
        return Math.Round(subtotal * 0.1m, 0);
    }

    private async Task<UserAddress> ResolveShippingAddressAsync(
        int userId,
        CreateOrderRequest request
    )
    {
        var hasAddressId = request.ShippingAddressId.HasValue;
        var hasNewAddress = request.NewShippingAddress != null;

        if (hasAddressId == hasNewAddress)
        {
            throw new BadRequestException(
                "Provide exactly one of shippingAddressId or newShippingAddress."
            );
        }

        if (hasAddressId)
        {
            var existingAddresses = await _unitOfWork.UserAddresses.FindAsync(a =>
                a.AddressId == request.ShippingAddressId!.Value && a.UserId == userId
            );

            return existingAddresses.FirstOrDefault()
                ?? throw new NotFoundException("Shipping address not found");
        }

        var newAddress = request.NewShippingAddress!;

        var user =
            await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var addressEntity = UserAddress.CreateDefault(
            user: user,
            type: newAddress.Type,
            label: string.IsNullOrWhiteSpace(newAddress.Label)
                ? "Shipping"
                : newAddress.Label.Trim(),
            recipientName: newAddress.RecipientName.Trim(),
            phone: newAddress.Phone.Trim(),
            addressLine1: newAddress.AddressLine1.Trim(),
            city: newAddress.City.Trim(),
            stateProvince: newAddress.StateProvince.Trim(),
            postalCode: newAddress.PostalCode.Trim(),
            country: newAddress.Country.Trim(),
            addressLine2: string.IsNullOrWhiteSpace(newAddress.AddressLine2)
                ? null
                : newAddress.AddressLine2.Trim(),
            isDefaultShipping: false,
            isDefaultBilling: false
        );

        // Persist only when requested. Otherwise use one-time address for this order only.
        if (request.SaveNewShippingAddress)
        {
            await _unitOfWork.UserAddresses.AddAsync(addressEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        return addressEntity;
    }

    private async Task<(
        int? CouponId,
        string? CouponCode,
        decimal CouponDiscount
    )> ResolveCouponAsync(
        string? couponCodeInput,
        decimal subtotal,
        decimal shippingFee,
        decimal taxAmount
    )
    {
        if (string.IsNullOrWhiteSpace(couponCodeInput))
            return (null, null, 0m);

        var code = couponCodeInput.Trim().ToUpperInvariant();

        var coupon =
            await _couponRepository.GetByCodeAsync(code)
            ?? throw new BadRequestException("Invalid coupon code");

        if (!coupon.IsActive)
            throw new BadRequestException("Coupon is inactive");

        var now = DateTime.UtcNow;
        if (coupon.ValidFrom.HasValue && now < coupon.ValidFrom.Value)
            throw new BadRequestException("Coupon is not active yet");

        if (coupon.ValidUntil.HasValue && now > coupon.ValidUntil.Value)
            throw new BadRequestException("Coupon has expired");

        var orderTotalBeforeDiscount = subtotal + shippingFee + taxAmount;

        if (
            coupon.MinOrderAmount.HasValue
            && orderTotalBeforeDiscount < coupon.MinOrderAmount.Value
        )
            throw new BadRequestException("Order does not meet coupon minimum amount");

        var usageCount = await _couponRepository.CountUsageAsync(coupon.CouponId);

        if (usageCount >= 1)
            throw new BadRequestException("Coupon has already been used");

        if (coupon.UsageLimit.HasValue && usageCount >= coupon.UsageLimit.Value)
            throw new BadRequestException("Coupon usage limit reached");

        decimal discount = coupon.DiscountType switch
        {
            DiscountType.percentage => CalculatePercentageDiscount(
                coupon.DiscountValue,
                orderTotalBeforeDiscount
            ),
            DiscountType.fixed_amount => CalculateFixedAmountDiscount(
                coupon.DiscountValue,
                orderTotalBeforeDiscount
            ),
            DiscountType.free_shipping => shippingFee,
            _ => throw new BadRequestException("Invalid coupon type"),
        };

        if (discount > orderTotalBeforeDiscount)
            discount = orderTotalBeforeDiscount;

        return (coupon.CouponId, coupon.Code, discount);
    }

    private static decimal CalculatePercentageDiscount(decimal percentage, decimal total)
    {
        if (percentage <= 0 || percentage > 20)
            throw new BadRequestException(
                "Percentage coupon must be greater than 0 and at most 20%"
            );
        return Math.Round(total * (percentage / 100m), 2);
    }

    private static decimal CalculateFixedAmountDiscount(decimal amount, decimal total)
    {
        if (amount <= 0)
            throw new BadRequestException("Fixed amount coupon must be greater than 0");

        var maxAllowed = total / 2m;
        if (amount > maxAllowed)
            throw new BadRequestException(
                "Fixed amount coupon cannot exceed half of the order total"
            );

        return amount;
    }
}
