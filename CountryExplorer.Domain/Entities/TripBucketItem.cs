using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Domain.Entities;

public class TripBucketItem
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public TripStatus Status { get; set; } = TripStatus.Planned;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? VisitedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public User User { get; set; } = null!;
    public string? GoogleEventId { get; set; }
}