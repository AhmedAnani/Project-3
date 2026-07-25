using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Application.DTOs.Trip;

public class TripItemResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public TripStatus Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? VisitedDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? GoogleEventId { get; set; }
}
