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
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher,
    IMapper mapper) : IOrderService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly IMapper _mapper = mapper;

    public async Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request)
    {
        // Get user's cart with details
        var cart = await _cartRepository.GetByUserIdWithDetailsAsync(userId)
            ?? throw new NotFoundException("Cart not found");

        if (cart.CartItems.Count == 0)
            throw new BadRequestException("Cart is empty");

        // Validate all items are still available
        foreach (var item in cart.CartItems)
        {
            var availableStock = item.Sku?.Inventory?.QuantityAvailable ?? 0;
            if (availableStock < item.Quantity)
            {
                throw new BadRequestException(
                    $"Insufficient stock for {item.Sku?.Product?.ProductName ?? "item"}. " +
                    $"Available: {availableStock}, Requested: {item.Quantity}");
            }
        }

        // Calculate order totals
        var subtotal = cart.CartItems.Sum(i => i.PriceSnapshot * i.Quantity);
        var shippingFee = CalculateShippingFee(subtotal);
        var taxAmount = CalculateTax(subtotal);
        var couponDiscount = 0m; // TODO: Implement coupon logic
        var totalAmount = subtotal + shippingFee + taxAmount - couponDiscount;

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
                couponCode: request.CouponCode,
                couponDiscount: couponDiscount,
                customerNotes: request.CustomerNotes);

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
                    variantDescription: cartItem.Sku?.VariantAttributes);
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
                addressLine2: shippingAddress.AddressLine2);
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
                Items = order.OrderItems.Select(i => new OrderItemEvent
                {
                    SkuId = i.SkuId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    SellerId = i.SellerId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            };

            await _eventPublisher.PublishAsync(orderCreatedEvent);

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

    public async Task<PagedResult<OrderSummaryDto>> GetUserOrdersAsync(int userId, PaginationParams paginationParams)
    {
        var (orders, totalCount) = await _orderRepository.GetOrdersByUserIdAsync(
            userId,
            paginationParams.PageNumber,
            paginationParams.PageSize);

        var orderSummaries = _mapper.Map<List<OrderSummaryDto>>(orders);

        return PagedResult<OrderSummaryDto>.Create(
            orderSummaries,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            totalCount);
    }

    public async Task<OrderDto?> CancelAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);

        if (order == null || order.UserId != userId)
            return null;

        if (order.Status != OrderStatus.created && order.Status != OrderStatus.confirmed)
            throw new BadRequestException("Order cannot be cancelled at this stage");

        order.Status = OrderStatus.cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

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

    private async Task<UserAddress> ResolveShippingAddressAsync(int userId, CreateOrderRequest request)
    {
        var hasAddressId = request.ShippingAddressId.HasValue;
        var hasNewAddress = request.NewShippingAddress != null;

        if (hasAddressId == hasNewAddress)
        {
            throw new BadRequestException(
                "Provide exactly one of shippingAddressId or newShippingAddress.");
        }

        if (hasAddressId)
        {
            var existingAddresses = await _unitOfWork.UserAddresses.FindAsync(
                a => a.AddressId == request.ShippingAddressId!.Value && a.UserId == userId);

            return existingAddresses.FirstOrDefault()
                ?? throw new NotFoundException("Shipping address not found");
        }

        var newAddress = request.NewShippingAddress!;

        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        var addressEntity = UserAddress.CreateDefault(
            user: user,
            type: newAddress.Type,
            label: string.IsNullOrWhiteSpace(newAddress.Label) ? "Shipping" : newAddress.Label.Trim(),
            recipientName: newAddress.RecipientName.Trim(),
            phone: newAddress.Phone.Trim(),
            addressLine1: newAddress.AddressLine1.Trim(),
            city: newAddress.City.Trim(),
            stateProvince: newAddress.StateProvince.Trim(),
            postalCode: newAddress.PostalCode.Trim(),
            country: newAddress.Country.Trim(),
            addressLine2: string.IsNullOrWhiteSpace(newAddress.AddressLine2) ? null : newAddress.AddressLine2.Trim(),
            isDefaultShipping: false,
            isDefaultBilling: false);

        // Persist only when requested. Otherwise use one-time address for this order only.
        if (request.SaveNewShippingAddress)
        {
            await _unitOfWork.UserAddresses.AddAsync(addressEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        return addressEntity;
    }
}
