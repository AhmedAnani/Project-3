using CountryExplorer.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CountryExplorer.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public required string FullName { get; set; }

    public string? PictureUrl { get; set; }

    public string? GoogleId { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public ICollection<TripBucketItem> TripBucketItems { get; set; } = [];
}