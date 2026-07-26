using CountryExplorer.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CountryExplorer.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException valEx)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = valEx.Errors.Select(e => e.ErrorMessage).ToList();

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = string.Join("; ", errors)
            }, cancellationToken);

            return true;
        }


        // Rate limiting need a special header ,handle it separately
        if (exception is RateLimitExceededException rateEx)
        {
            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.Headers.Append("Retry-After", rateEx.RetryAfterSeconds.ToString());

            _logger.LogWarning(exception, "Rate limit exceeded.");

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = rateEx.Message
            }, cancellationToken);

            return true;
        }

        var (statusCode, title) = exception switch
        {
            CountryNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            AttractionNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            CurrencyNotSupportedException => (StatusCodes.Status400BadRequest, exception.Message),
            InvalidTokenException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            ExternalServiceUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var detail = exception.ToString();

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Extensions = { ["traceId"] = httpContext.TraceIdentifier }
        }, cancellationToken);

        return true;
    }
}