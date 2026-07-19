using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Application.DTOs;

public class TripItemResponseDto
{
    public int Id { get; set; }

    public string CountryCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public TripStatus Status { get; set; }

    public DateTime? TargetDate { get; set; }

    public DateTime? VisitedDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? GoogleEventId { get; set; }
}
