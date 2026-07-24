namespace CountryExplorer.Application.Interfaces.External;

/// <summary>
/// Service interface for managing Google Calendar integration with trips.
/// Accepts primitives to prevent domain entity leakage into external service abstractions.
/// </summary>
public interface IGoogleCalendarService
{
    /// <summary>
    /// Schedules a trip event in Google Calendar.
    /// </summary>
    Task<string?> ScheduleTripEventAsync(
        string googleAccessToken,
        string title,
        string? notes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing trip event in Google Calendar.
    /// </summary>
    Task<bool> UpdateTripEventAsync(
        string googleAccessToken,
        string googleEventId,
        string title,
        string? notes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a trip event from Google Calendar.
    /// </summary>
    Task<bool> DeleteTripEventAsync(
        string googleAccessToken,
        string googleEventId,
        CancellationToken cancellationToken = default);
}
