using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.inventory
{
    public class InventoryUpdateDto
    {
        [Range(0, int.MaxValue)]
        public int? QuantityAvailable { get; set; }

        [Range(0, int.MaxValue)]
        public int? QuantityReserved { get; set; }

        [Range(0, int.MaxValue)]
        public int? QuantitySold { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReorderPoint { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReorderQuantity { get; set; }
    }
}