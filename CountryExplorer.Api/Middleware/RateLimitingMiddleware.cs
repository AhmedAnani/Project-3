using CountryExplorer.Domain.Exceptions;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace CountryExplorer.Api.Middleware;

/// <summary>
/// Middleware that limits the number of requests per client to a maximum within a sliding time window.
/// Clients are identified by authenticated user ID or IP address.
/// Configuration: RateLimit:MaxRequests and RateLimit:WindowSeconds in appsettings.json
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new();

    private const int MaxRequests = 10;
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(10);

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware. Tracks request timestamps per client and rejects excess requests.
    /// </summary>
    /// <exception cref="RateLimitExceededException">Thrown when the client exceeds the request limit.</exception>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var now = DateTime.UtcNow;

        var queue = _requests.GetOrAdd(clientId, _ => new Queue<DateTime>());

        lock (queue)
        {
            // Remove old requests
            while (queue.Count > 0 && now - queue.Peek() > Window)
                queue.Dequeue();

            if (queue.Count >= MaxRequests)
                throw new RateLimitExceededException(
                    $"Too many requests. Please wait {Window.TotalSeconds} seconds.",
                    (int)Window.TotalSeconds);

            queue.Enqueue(now);
        }

        await _next(context);
    }

    private static string GetClientId(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) ? $"user:{userId}" : $"ip:{context.Connection.RemoteIpAddress}";
    }
}


    