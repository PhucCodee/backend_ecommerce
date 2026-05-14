using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.inventory;

namespace ECommerce.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryDto> UpdateAsync(int skuId, InventoryUpdateDto updateDto, int sellerId);

        /// <summary>
        /// Release reserved stock for the given order back to "available".
        /// Used when an order is cancelled so the items go back on sale.
        /// Called from within an existing transaction — does NOT call SaveChanges itself.
        /// </summary>
        /// <param name="reservations">Map of skuId → quantity to release.</param>
        Task ReleaseReservationAsync(
            int orderId,
            string orderNumber,
            IEnumerable<(int SkuId, int Quantity)> reservations
        );
    }
}
