using System;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.DTOs.inventory;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class InventoryService(
        IProductSkuRepository productSkuRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IInventoryService
    {
        private readonly IProductSkuRepository _productSkuRepository = productSkuRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<InventoryDto> UpdateAsync(int skuId, InventoryUpdateDto updateDto, int sellerId)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");
        
            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new NotFoundException("Product SKU not found");
        
            if (sku.Product.SellerId != sellerId)
                throw new ForbiddenException("You do not have permission to update inventory for this SKU");
        
            var now = DateTime.UtcNow;
        
            if (sku.Inventory == null)
                sku.Inventory = Inventory.CreateDefault(sku, 0);
        
            var inventory = sku.Inventory;
        
            var quantityAvailable = updateDto.QuantityAvailable ?? inventory.QuantityAvailable;
            var quantityReserved = updateDto.QuantityReserved ?? inventory.QuantityReserved;
            var quantitySold = updateDto.QuantitySold ?? inventory.QuantitySold;
            var reorderPoint = updateDto.ReorderPoint ?? inventory.ReorderPoint;
            var reorderQuantity = updateDto.ReorderQuantity ?? inventory.ReorderQuantity;
        
            if (quantityAvailable < 0 || quantityReserved < 0 || quantitySold < 0 ||
                reorderPoint < 0 || reorderQuantity < 0)
                throw new BadRequestException("Inventory values must be non-negative");
        
            if (quantityReserved > quantityAvailable)
                throw new BadRequestException("Reserved quantity cannot exceed available quantity");
        
            if (updateDto.QuantityAvailable.HasValue && quantityAvailable > inventory.QuantityAvailable)
                inventory.LastRestockedAt = now;
        
            inventory.QuantityAvailable = quantityAvailable;
            inventory.QuantityReserved = quantityReserved;
            inventory.QuantitySold = quantitySold;
            inventory.ReorderPoint = reorderPoint;
            inventory.ReorderQuantity = reorderQuantity;
            inventory.UpdatedAt = now;
        
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InventoryDto>(inventory);
        }
    }
}