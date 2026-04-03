using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.DTOs.cart;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class CartService(
        ICartRepository cartRepository,
        IProductSkuRepository productSkuRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : ICartService
    {
        private readonly ICartRepository _cartRepository = cartRepository;
        private readonly IProductSkuRepository _productSkuRepository = productSkuRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<CartDto> GetCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> AddToCartAsync(int? userId, string? sessionId, AddToCartDto addDto, string? ipAddress = null)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(addDto.SkuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new BadRequestException("Product is not available");

            var availableStock = sku.Inventory?.QuantityAvailable ?? 0;
            if (availableStock < addDto.Quantity)
                throw new BadRequestException($"Insufficient stock. Available: {availableStock}");

            var cart = await GetOrCreateCartAsync(userId, sessionId, ipAddress);

            // Check if item already in cart
            var existingItem = await _cartRepository.GetCartItemAsync(cart.CartId, addDto.SkuId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + addDto.Quantity;
                if (newQuantity > availableStock)
                    throw new BadRequestException($"Cannot add more items. Available: {availableStock}, In cart: {existingItem.Quantity}");

                existingItem.Quantity = newQuantity;
                existingItem.PriceSnapshot = sku.Price;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = CartItem.CreateDefault(cart, sku, addDto.Quantity);
                await _cartRepository.AddCartItemAsync(cartItem);
            }

            cart.RecalculateTotals();
            await _unitOfWork.SaveChangesAsync();

            var updatedCart = await GetCartWithDetailsAsync(userId, sessionId);
            return _mapper.Map<CartDto>(updatedCart);
        }

        public async Task<CartDto> UpdateCartItemAsync(int? userId, string? sessionId, int cartItemId, UpdateCartItemDto updateDto)
        {
            var cart = await GetCartWithDetailsAsync(userId, sessionId)
                ?? throw new NotFoundException("Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId)
                ?? throw new NotFoundException("Cart item not found");

            var availableStock = cartItem.Sku.Inventory?.QuantityAvailable ?? 0;
            if (updateDto.Quantity > availableStock)
                throw new BadRequestException($"Insufficient stock. Available: {availableStock}");

            cartItem.Quantity = updateDto.Quantity;
            cartItem.PriceSnapshot = cartItem.Sku.Price;
            cartItem.UpdatedAt = DateTime.UtcNow;

            cart.RecalculateTotals();
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> RemoveCartItemAsync(int? userId, string? sessionId, int cartItemId)
        {
            var cart = await GetCartWithDetailsAsync(userId, sessionId)
                ?? throw new NotFoundException("Cart not found");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId)
                ?? throw new NotFoundException("Cart item not found");

            cart.CartItems.Remove(cartItem);
            await _cartRepository.RemoveCartItemAsync(cartItem);

            cart.RecalculateTotals();
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<bool> ClearCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetCartWithDetailsAsync(userId, sessionId);

            if (cart == null)
                return true;

            foreach (var item in cart.CartItems.ToList())
            {
                await _cartRepository.RemoveCartItemAsync(item);
            }

            cart.CartItems.Clear();
            cart.RecalculateTotals();
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CartDto> MergeCartsAsync(int userId, string sessionId)
        {
            var guestCart = await _cartRepository.GetBySessionIdWithDetailsAsync(sessionId);
            var userCart = await _cartRepository.GetByUserIdWithDetailsAsync(userId);

            if (guestCart == null && userCart == null)
            {
                var newCart = Cart.CreateDefault(userId: userId);
                await _cartRepository.AddAsync(newCart);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<CartDto>(newCart);
            }

            if (guestCart == null)
            {
                return _mapper.Map<CartDto>(userCart!);
            }

            if (userCart == null)
            {
                guestCart.UserId = userId;
                guestCart.SessionId = null;
                guestCart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<CartDto>(guestCart);
            }

            // Merge guest cart items into user cart
            foreach (var guestItem in guestCart.CartItems)
            {
                var existingItem = userCart.CartItems.FirstOrDefault(ci => ci.SkuId == guestItem.SkuId);

                if (existingItem != null)
                {
                    var sku = await _productSkuRepository.GetByIdWithDetailsAsync(guestItem.SkuId);
                    var availableStock = sku?.Inventory?.QuantityAvailable ?? 0;
                    var newQuantity = Math.Min(existingItem.Quantity + guestItem.Quantity, availableStock);
                    existingItem.Quantity = newQuantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Cart = userCart,
                        CartId = userCart.CartId,
                        Sku = guestItem.Sku,
                        SkuId = guestItem.SkuId,
                        Quantity = guestItem.Quantity,
                        PriceSnapshot = guestItem.PriceSnapshot,
                        AddedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _cartRepository.AddCartItemAsync(newItem);
                }
            }

            // Mark guest cart as converted
            guestCart.Status = CartStatus.converted;
            guestCart.UpdatedAt = DateTime.UtcNow;

            userCart.RecalculateTotals();
            await _unitOfWork.SaveChangesAsync();

            // Reload the cart to get clean state after merge
            var mergedCart = await _cartRepository.GetByUserIdWithDetailsAsync(userId);
            return _mapper.Map<CartDto>(mergedCart!);
        }

        private async Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId, string? ipAddress = null)
        {
            Cart? cart = null;

            if (userId.HasValue)
            {
                cart = await _cartRepository.GetByUserIdWithDetailsAsync(userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await _cartRepository.GetBySessionIdWithDetailsAsync(sessionId);
            }

            if (cart == null)
            {
                cart = Cart.CreateDefault(userId, sessionId, ipAddress);
                await _cartRepository.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<Cart?> GetCartWithDetailsAsync(int? userId, string? sessionId)
        {
            if (userId.HasValue)
            {
                return await _cartRepository.GetByUserIdWithDetailsAsync(userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                return await _cartRepository.GetBySessionIdWithDetailsAsync(sessionId);
            }

            return null;
        }
    }
}