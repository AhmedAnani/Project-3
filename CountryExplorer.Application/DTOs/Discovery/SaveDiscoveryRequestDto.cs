namespace CountryExplorer.Application.DTOs.Discovery;

/// <summary>
/// Input DTO for saving a discovered destination to the user's trip bucket list.
/// Allows the frontend to optionally pass travel dates, falling back to smart defaults if omitted.
/// </summary>
public class SaveDiscoveryRequestDto
{
    /// <summary>
    /// Optional start date of the trip.
    /// If null, defaults to 30 days from now.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional end date of the trip.
    /// If null, defaults to StartDate + 7 days.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
