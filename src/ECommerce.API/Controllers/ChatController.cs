using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    ) : ControllerBase
    {
        private readonly string _aiBaseUrl =
            configuration["Chatbot:BaseUrl"] ?? "http://chatbot_api:8000";
        private readonly HttpClient _http = httpClientFactory.CreateClient();

        public record ChatRequest(string Message);

        public record ChatResponse(
            string Text,
            string? Intent,
            object? Data,
            string? SessionId
        );

        [EnableRateLimiting("ApiPolicy")]
        [HttpPost]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest request,
            CancellationToken ct
        )
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(
                    ApiResponse<object>.Fail("Message is required")
                );

            var url = _aiBaseUrl.TrimEnd('/') + "/api/ai/chat";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsed) ? parsed : 0;

            HttpResponseMessage upstream;
            try
            {
                upstream = await _http.PostAsJsonAsync(
                    url,
                    new { user_id = userId, message = request.Message },
                    ct
                );
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(
                    502,
                    ApiResponse<object>.Fail($"AI service unreachable: {ex.Message}")
                );
            }
            catch (TaskCanceledException)
            {
                return StatusCode(
                    504,
                    ApiResponse<object>.Fail("AI service timed out")
                );
            }

            using (upstream)
            {
                if (!upstream.IsSuccessStatusCode)
                {
                    var body = await upstream.Content.ReadAsStringAsync(ct);
                    return StatusCode(
                        (int)upstream.StatusCode,
                        ApiResponse<object>.Fail(
                            string.IsNullOrWhiteSpace(body)
                                ? $"AI service returned {(int)upstream.StatusCode}"
                                : body
                        )
                    );
                }

                ChatResponse? data;
                try
                {
                    data = await upstream.Content.ReadFromJsonAsync<ChatResponse>(
                        new JsonSerializerOptions(JsonSerializerDefaults.Web),
                        ct
                    );
                }
                catch (Exception ex)
                {
                    return StatusCode(
                        502,
                        ApiResponse<object>.Fail($"AI service returned malformed JSON: {ex.Message}")
                    );
                }

                if (data is null)
                    return StatusCode(
                        502,
                        ApiResponse<object>.Fail("AI service returned empty response")
                    );

                return Ok(ApiResponse<ChatResponse>.Ok(data));
            }
        }
    }
}
