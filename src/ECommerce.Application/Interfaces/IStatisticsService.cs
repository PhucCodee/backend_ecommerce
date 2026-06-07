using System.Threading.Tasks;
using ECommerce.Application.DTOs.statistics;

namespace ECommerce.Application.Interfaces
{
    public interface IStatisticsService
    {
        /// <param name="sellerId">Authenticated seller user id (maps to Product.SellerId and OrderItem.SellerId).</param>
        Task<SellerDashboardDto> GetSellerDashboardAsync(int sellerId, int trendDays = 30);
        Task<AdminDashboardDto> GetAdminDashboardAsync(int trendDays = 30);
    }
}
