using System;
using ECommerce.API.Logging;
using ECommerce.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.AdminOnly)]
    public class LogsController(InMemoryLogBuffer buffer) : ControllerBase
    {
        private readonly InMemoryLogBuffer _buffer = buffer;

        /// <summary>
        /// GET /api/logs?level=Warning&limit=100
        /// </summary>
        [HttpGet]
        public IActionResult GetLogs([FromQuery] string? level, [FromQuery] int limit = 100)
        {
            limit = Math.Clamp(limit, 1, 300);
            var entries = _buffer.GetRecent(limit, level);
            var counts = _buffer.LevelCounts();

            return Ok(new
            {
                total = _buffer.TotalCount,
                counts,
                entries
            });
        }
    }
}
