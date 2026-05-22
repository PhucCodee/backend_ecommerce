using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using ECommerce.Application.Common.Authorization;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController(
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory
    ) : ControllerBase
    {
        private readonly ApplicationDbContext _db = dbContext;
        private readonly IConfiguration _config = configuration;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var checks = new List<object>();
            var overall = "healthy";
            var sw = Stopwatch.StartNew();

            // 1. Database check
            var dbSw = Stopwatch.StartNew();
            string dbStatus;
            string? dbError = null;
            try
            {
                await _db.Database.ExecuteSqlRawAsync("SELECT 1");
                dbStatus = "healthy";
            }
            catch (Exception ex)
            {
                dbStatus = "unhealthy";
                dbError = ex.Message;
                overall = "unhealthy";
            }
            dbSw.Stop();
            checks.Add(
                new
                {
                    name = "database",
                    status = dbStatus,
                    durationMs = dbSw.Elapsed.TotalMilliseconds,
                    description = "PostgreSQL database connection",
                    error = dbError,
                }
            );

            // 2. RabbitMQ management API check
            var mqSw = Stopwatch.StartNew();
            string mqStatus;
            string? mqError = null;
            string mqVersion = "unknown";
            try
            {
                var mqHost = string.IsNullOrWhiteSpace(_config["RabbitMQ:Host"]) ? "message_broker" : _config["RabbitMQ:Host"]!;
                var mqUser = string.IsNullOrWhiteSpace(_config["RabbitMQ:User"]) ? "guest" : _config["RabbitMQ:User"]!;
                var mqPass = string.IsNullOrWhiteSpace(_config["RabbitMQ:Password"]) ? "guest" : _config["RabbitMQ:Password"]!;
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{mqUser}:{mqPass}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(byteArray)
                    );
                var response = await client.GetAsync($"http://{mqHost}:15672/api/overview");
                if (response.IsSuccessStatusCode)
                {
                    mqStatus = "healthy";
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("rabbitmq_version", out var v))
                        mqVersion = v.GetString() ?? "unknown";
                }
                else
                {
                    mqStatus = "degraded";
                    mqError = $"HTTP {(int)response.StatusCode}";
                    if (overall == "healthy")
                        overall = "degraded";
                }
            }
            catch (Exception ex)
            {
                mqStatus = "unhealthy";
                mqError = ex.Message;
                overall = "unhealthy";
            }
            mqSw.Stop();
            checks.Add(
                new
                {
                    name = "rabbitmq",
                    status = mqStatus,
                    durationMs = mqSw.Elapsed.TotalMilliseconds,
                    description = $"RabbitMQ message broker (v{mqVersion})",
                    error = mqError,
                }
            );

            // 3. API itself
            checks.Add(
                new
                {
                    name = "api",
                    status = "healthy",
                    durationMs = 0.0,
                    description = "ECommerce REST API",
                    error = (string?)null,
                }
            );

            sw.Stop();

            var result = new
            {
                status = overall,
                totalDurationMs = sw.Elapsed.TotalMilliseconds,
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                checks,
            };

            var statusCode =
                overall == "healthy" ? 200
                : overall == "degraded" ? 200
                : 503;
            return StatusCode(statusCode, result);
        }

        /// <summary>
        /// Admin-only proxy for the RabbitMQ Management API queues endpoint.
        /// Browser CORS blocks calling http://localhost:15672 directly, so the
        /// frontend hits this endpoint instead and we make the call server-side.
        /// </summary>
        [HttpGet("rabbitmq/queues")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetRabbitMqQueues()
        {
            try
            {
                var mqHost = string.IsNullOrWhiteSpace(_config["RabbitMQ:Host"]) ? "message_broker" : _config["RabbitMQ:Host"]!;
                var mqUser = string.IsNullOrWhiteSpace(_config["RabbitMQ:User"]) ? "guest" : _config["RabbitMQ:User"]!;
                var mqPass = string.IsNullOrWhiteSpace(_config["RabbitMQ:Password"]) ? "guest" : _config["RabbitMQ:Password"]!;
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{mqUser}:{mqPass}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(byteArray)
                    );
                var response = await client.GetAsync($"http://{mqHost}:15672/api/queues");
                if (!response.IsSuccessStatusCode)
                    return StatusCode(
                        (int)response.StatusCode,
                        new { reachable = false, queues = Array.Empty<object>() }
                    );

                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Ok(
                    new
                    {
                        reachable = false,
                        error = ex.Message,
                        queues = Array.Empty<object>(),
                    }
                );
            }
        }
    }
}
