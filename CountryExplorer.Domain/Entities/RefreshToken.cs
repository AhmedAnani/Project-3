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
    /// The token string value (
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
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When this token was revoked .
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Indicates whether this token is currently active 
    /// </summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiresAt;

    /// <summary>
    /// Creates a refresh token with proper timestamp initialization.
    /// </summary>
    public static RefreshToken Create(Guid userId, string tokenValue)
    {
        var now = DateTime.UtcNow;
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = tokenValue,
            UserId = userId,
            CreatedAt = now,
            ExpiresAt = now.AddDays((int)ExpirationTime.RefreshToken),
            RevokedAt = null
        };
    }

    /// <summary>
    /// Revokes the token instead of deleting it 
    /// </summary>
    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}