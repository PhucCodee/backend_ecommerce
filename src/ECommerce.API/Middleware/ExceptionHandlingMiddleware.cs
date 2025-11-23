using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.common;
using System.Collections.Generic;

namespace ECommerce.API.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);

            var (statusCode, message) = ex switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Access denied"),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Failure(message, (int)statusCode);
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}