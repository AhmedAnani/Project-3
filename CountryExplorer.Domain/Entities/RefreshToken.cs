using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Domain.Entities;

/// <summary>
/// Represents a refresh token used for obtaining new access tokens.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The token string value (base64 encoded).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the User entity.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// When this token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this token expires (30 days from creation).
    /// </summary>
    public DateTime ExpiresAt { get; set; } =
        DateTime.UtcNow.AddDays((int)ExpirationTime.RefreshToken);

    /// <summary>
    /// When this token was revoked (null if still active).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Indicates whether this token is currently active.
    /// </summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiresAt;
}