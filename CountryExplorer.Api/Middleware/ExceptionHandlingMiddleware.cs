using CountryExplorer.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Project_3.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions and returns standardized error responses.
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

    /// <summary>
    /// Invokes the middleware to catch and handle exceptions.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}",
                context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles the exception and writes a standardized error response.
    /// </summary>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = GetErrorResponse(context, exception);
        context.Response.StatusCode = response.StatusCode;

        if (exception is RateLimitExceededException rateEx)
        {
            context.Response.Headers.Append("Retry-After",
                rateEx.RetryAfterSeconds.ToString());
        }

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Builds a standardized error response based on exception type.
    /// </summary>
    private ErrorResponse GetErrorResponse(HttpContext context, Exception exception)
    {
        return exception switch
        {
            RateLimitExceededException rateEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status429TooManyRequests,
                ErrorCode = "RATE_LIMIT_EXCEEDED",
                Message = rateEx.Message,
                TraceId = context.TraceIdentifier,
                Detail = null
            },

            InvalidTokenException tokenEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ErrorCode = "INVALID_TOKEN",
                Message = tokenEx.Message,
                TraceId = context.TraceIdentifier,
                Detail = _env.IsDevelopment() ? tokenEx.InnerException?.Message : null
            },

            ArgumentNullException argEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = "INVALID_REQUEST",
                Message = $"Missing required parameter: {argEx.ParamName}",
                TraceId = context.TraceIdentifier,
                Detail = null
            },

            _ => new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = "INTERNAL_ERROR",
                Message = _env.IsDevelopment()
                    ? exception.Message
                    : "An error occurred while processing your request.",
                TraceId = context.TraceIdentifier,
                Detail = _env.IsDevelopment() ? exception.StackTrace : null
            }
        };
    }
}

/// <summary>
/// Standardized error response format.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? Detail { get; set; }
}