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
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestLogs = new();
    private static readonly SemaphoreSlim _cleanupLock = new(1, 1);
    private static long _lastCleanupTicks = DateTime.UtcNow.Ticks;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger,
        IConfiguration config)
    {
        _next = next;
        _logger = logger;

        // Read from configuration with defaults
        _maxRequests = config.GetValue<int>("RateLimit:MaxRequests", 10);
        int windowSeconds = config.GetValue<int>("RateLimit:WindowSeconds", 10);
        _window = TimeSpan.FromSeconds(windowSeconds);

        _logger.LogInformation("Rate limiting initialized. MaxRequests: {MaxRequests}, Window: {Window}s",
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

        // Periodic cleanup to free memory
        await CleanupAsync(now);

        var queue = _requestLogs.GetOrAdd(clientId, _ => new Queue<DateTime>());

        lock (queue)
        {
            // Remove timestamps older than the window
            while (queue.Count > 0 && now - queue.Peek() > _window)
            {
                queue.Dequeue();
            }

            if (queue.Count >= _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}. Queue size: {QueueSize}",
                    clientId, queue.Count);

                throw new RateLimitExceededException(
                    $"Too many requests. Please wait {_window.TotalSeconds} seconds before retrying.",
                    (int)_window.TotalSeconds);
            }

            // Allow the request
            queue.Enqueue(now);
        }

        await _next(context);
    }

    /// <summary>
    /// Gets a unique identifier for the client based on authenticated user or IP address.
    /// </summary>
    private static string GetClientId(HttpContext context)
    {
        // Prefer authenticated user ID
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        // Fall back to IP address
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    /// <summary>
    /// Periodically removes expired entries from the request log to prevent memory leaks.
    /// </summary>
    private async Task CleanupAsync(DateTime now)
    {
        // Check if cleanup needed (using Interlocked for better performance)
        long lastCleanupTicks = Interlocked.Read(ref _lastCleanupTicks);
        if (now.Ticks - lastCleanupTicks < TimeSpan.FromSeconds(5).Ticks)
            return;

        // Acquire lock for cleanup
        if (!await _cleanupLock.WaitAsync(0))
            return; // Skip if lock is busy

        try
        {
            // Double-check pattern
            lastCleanupTicks = Interlocked.Read(ref _lastCleanupTicks);
            if (now.Ticks - lastCleanupTicks < TimeSpan.FromSeconds(5).Ticks)
                return;

            var cutoff = now - _window;
            var keysToRemove = new List<string>();

            foreach (var kvp in _requestLogs)
            {
                lock (kvp.Value)
                {
                    while (kvp.Value.Count > 0 && kvp.Value.Peek() < cutoff)
                        kvp.Value.Dequeue();

                    if (kvp.Value.Count == 0)
                        keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (_requestLogs.TryRemove(key, out _))
                {
                    _logger.LogDebug("Cleaned up rate limit entry for client: {ClientId}", key);
                }
            }

            Interlocked.Exchange(ref _lastCleanupTicks, now.Ticks);
            _logger.LogDebug("Rate limit cleanup completed. Entries removed: {Count}", keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during rate limit cleanup");
        }
        finally
        {
            _cleanupLock.Release();
        }
    }
}