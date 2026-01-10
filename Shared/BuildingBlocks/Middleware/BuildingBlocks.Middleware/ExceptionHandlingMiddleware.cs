using BuildingBlocks.Middleware.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace BuildingBlocks.Middleware
{

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message, errors) = exception switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,
                    "Unauthorized access", new List<string> { exception.Message }),
                KeyNotFoundException => (HttpStatusCode.NotFound,
                    "Resource not found", new List<string> { exception.Message }),
                ArgumentException => (HttpStatusCode.BadRequest,
                    "Invalid argument", new List<string> { exception.Message }),
                InvalidOperationException => (HttpStatusCode.BadRequest,
                    "Invalid operation", new List<string> { exception.Message }),
                _ => (HttpStatusCode.InternalServerError,
                    "An error occurred while processing your request",
                    new List<string> { exception.Message })
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.ErrorResponse(message, errors);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    // Extension method for easy registration
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
