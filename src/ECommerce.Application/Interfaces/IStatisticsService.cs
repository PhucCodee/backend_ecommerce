using System.Threading.Tasks;
using ECommerce.Application.DTOs.statistics;

namespace ECommerce.Application.Interfaces
{
    public interface IStatisticsService
    {
        Task<SellerDashboardDto> GetSellerDashboardAsync(int sellerId, int trendDays = 30);
        Task<AdminDashboardDto> GetAdminDashboardAsync(int trendDays = 30);
    }
}
