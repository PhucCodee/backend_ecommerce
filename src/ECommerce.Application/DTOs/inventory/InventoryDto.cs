using System;

namespace ECommerce.Application.DTOs.inventory
{
    public class InventoryDto
    {
        public int InventoryId { get; set; }
        public int SkuId { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantitySold { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
        public DateTime? LastRestockedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}