using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.inventory
{
    public class InventoryCreateDto
    {
        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int QuantityReserved { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int QuantitySold { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int ReorderPoint { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int ReorderQuantity { get; set; } = 0;

        public DateTime? LastRestockedAt { get; set; }
    }
}