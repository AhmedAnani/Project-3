using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public ICollection<TripBucketItem> TripBucketItems { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}