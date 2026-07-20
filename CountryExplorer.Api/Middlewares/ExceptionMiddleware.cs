using CountryExplorer.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred.");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred.",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
