

namespace CountryExplorer.Application.Exceptions;

/// <summary>
/// Thrown when a client exceeds the allowed request limit within a time window.
/// </summary>
public class RateLimitExceededException : AppException
{
    /// <summary>
    /// The time (in seconds) after which the client can retry.
    /// </summary>
    public int RetryAfterSeconds { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitExceededException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="retryAfterSeconds">The number of seconds to wait before retrying.</param>
    public RateLimitExceededException(string message, int retryAfterSeconds)
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
