using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController(IStatisticsService statisticsService) : ControllerBase
    {
        private readonly IStatisticsService _statisticsService = statisticsService;

        /// <summary>
        /// Returns dashboard statistics for the authenticated seller.
        /// </summary>
        [HttpGet("seller")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> GetSellerDashboard([FromQuery] int trendDays = 30)
        {
            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _statisticsService.GetSellerDashboardAsync(sellerId, trendDays);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Returns platform-wide dashboard statistics for admins.
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAdminDashboard([FromQuery] int trendDays = 30)
        {
            var result = await _statisticsService.GetAdminDashboardAsync(trendDays);
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}
