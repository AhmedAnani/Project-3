using CountryExplorer.Application.Exceptions;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace CountryExplorer.Api.Middlewares;

/// <summary>
/// Middleware that limits the number of requests per client to a maximum within a sliding time window.
/// Clients are identified by authenticated user ID or IP address.
/// Configuration: RateLimit:MaxRequests and RateLimit:WindowSeconds in appsettings.json
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private static readonly ConcurrentDictionary<string, ClientRateLimit> _requests = new();
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

    public RateLimitingMiddleware(RequestDelegate next, IConfiguration config, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        _maxRequests = int.TryParse(config["RateLimit:MaxRequests"], out var max) ? max : 100;
        var windowSeconds = int.TryParse(config["RateLimit:WindowSeconds"], out var window) ? window : 60;
        _window = TimeSpan.FromSeconds(windowSeconds);

        _logger.LogInformation("Rate limiting configured: {MaxRequests} requests per {WindowSeconds} seconds",
            _maxRequests, windowSeconds);
    }

    /// <summary>
    /// Invokes the middleware. Tracks request timestamps per client and rejects excess requests.
    /// </summary>
    /// <exception cref="RateLimitExceededException">Thrown when the client exceeds the request limit.</exception>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var now = DateTime.UtcNow;

        if (now - _lastCleanup > CleanupInterval)
        {
            CleanupExpiredClients(now);
            _lastCleanup = now;
        }

        var clientLimit = _requests.GetOrAdd(clientId, _ => new ClientRateLimit());

        lock (clientLimit)
        {
            while (clientLimit.RequestTimes.Count > 0 &&
                   now - clientLimit.RequestTimes.Peek() > _window)
            {
                clientLimit.RequestTimes.Dequeue();
            }

            if (clientLimit.RequestTimes.Count >= _maxRequests)
            {
                var retryAfter = (int)Math.Ceiling(_window.TotalSeconds);
                _logger.LogWarning("Rate limit exceeded for client {ClientId}. Retry after {RetryAfter}s",
                    clientId, retryAfter);

                throw new RateLimitExceededException(
                    $"Too many requests. Please wait {retryAfter} seconds.",
                    retryAfter);
            }

            clientLimit.RequestTimes.Enqueue(now);
            clientLimit.LastActivityAt = now;
        }

        await _next(context);
    }

    /// <summary>
    /// Removes clients that have been inactive for longer than the rate limit window.
    /// Prevents unbounded memory growth.
    /// </summary>
    private static void CleanupExpiredClients(DateTime now)
    {
        var expiredClients = _requests
            .Where(x => now - x.Value.LastActivityAt > TimeSpan.FromHours(1))
            .Select(x => x.Key)
            .ToList();

        foreach (var client in expiredClients)
        {
            _requests.TryRemove(client, out _);
        }
    }

    private static string GetClientId(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId)
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress}";
    }

    /// <summary>
    /// Tracks request history for a single client.
    /// </summary>
    private class ClientRateLimit
    {
        public Queue<DateTime> RequestTimes { get; } = new();
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    }
}

