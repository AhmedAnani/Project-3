using CountryExplorer.Application.Exceptions;
using System.Text.Json;

namespace Project_3.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Map exception to status code and error code
        var (statusCode, errorCode, message, detail) = exception switch
        {
            RateLimitExceededException rate => (
                StatusCodes.Status429TooManyRequests,
                "RATE_LIMIT_EXCEEDED",
                rate.Message,
                (string?)null
            ),
            InvalidTokenException tokenEx => (
                StatusCodes.Status401Unauthorized,
                "INVALID_TOKEN",
                tokenEx.Message,
                _env.IsDevelopment() ? tokenEx.InnerException?.Message : null
            ),
            ArgumentNullException argEx => (
                StatusCodes.Status400BadRequest,
                "INVALID_REQUEST",
                $"Missing required parameter: {argEx.ParamName}",
                (string?)null
            ),
            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                "REQUEST_CANCELLED",
                "The request was cancelled.",
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                _env.IsDevelopment() ? exception.Message : "An error occurred while processing your request.",
                _env.IsDevelopment() ? exception.StackTrace : null
            )
        };

        context.Response.StatusCode = statusCode;

        // Retry-After header for rate limiting
        if (exception is RateLimitExceededException rateEx)
            context.Response.Headers.Append("Retry-After", rateEx.RetryAfterSeconds.ToString());

        var response = new
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = message,
            TraceId = context.TraceIdentifier,
            Detail = detail
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}