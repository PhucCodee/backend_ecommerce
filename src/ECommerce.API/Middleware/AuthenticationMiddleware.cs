using Microsoft.AspNetCore.Http;
using ECommerce.Infrastructure.Services;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ECommerce.API.Middleware
{
    public class AuthenticationMiddleware(RequestDelegate next, IJwtService jwtService)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                var jwtToken = token.Substring(7);
                var principal = jwtService.ValidateToken(jwtToken);

                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await next(context);
        }
    }
}