using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Exceptions;

namespace ECommerce.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (AppException ex)
            {
                await WriteError(context, ex.StatusCode, ex.Message);
            }
            catch (JsonException)
            {
                await WriteError(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Malformed JSON request body"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "[{TraceId}] Unhandled exception",
                    context.TraceIdentifier
                );

                await WriteError(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred"
                );
            }
        }

        private static async Task WriteError(
            HttpContext context,
            int statusCode,
            string message)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(
                ApiResponse<object>.Fail(message)
            );
        }
    }
}
