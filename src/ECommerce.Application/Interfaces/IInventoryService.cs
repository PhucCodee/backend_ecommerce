using System.Threading.Tasks;
using ECommerce.Application.DTOs.inventory;

namespace ECommerce.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryDto> UpdateAsync(int skuId, InventoryUpdateDto updateDto, int sellerId);
    }
}