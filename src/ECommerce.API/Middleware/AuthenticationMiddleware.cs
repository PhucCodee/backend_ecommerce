using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ECommerce.API.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for authentication token in the request headers
            var token = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                // Validate the token and set the user principal if valid
                // This is where you would add your token validation logic
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}