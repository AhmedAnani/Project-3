using CountryExplorer.Application.Exceptions;
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
    var (statusCode, title) = exception switch
    {
        CountryNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        AttractionNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        CurrencyNotSupportedException => (StatusCodes.Status400BadRequest, exception.Message),
        ExternalServiceUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };

    var detail = statusCode == StatusCodes.Status500InternalServerError
        ? "An unexpected error occurred."
        : exception.Message;

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
        Detail = detail
    }, cancellationToken);

    return true;
}
}