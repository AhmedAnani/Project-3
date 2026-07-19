using CountryExplorer.Domain.Entities;

namespace CountryExplorer.Application.Interfaces;

/// <summary>
/// Service interface for managing Google Calendar integration with trips.
/// </summary>
public interface IGoogleCalendarService
{
    /// <summary>
    /// Schedules a trip event in Google Calendar.
    /// </summary>
    /// <param name="googleAccessToken">Google OAuth access token.</param>
    /// <param name="trip">Trip details to schedule.</param>
    /// <returns>Google Calendar Event ID if successful; null otherwise.</returns>
    Task<string?> ScheduleTripEventAsync(string googleAccessToken, TripBucketItem trip);

    /// <summary>
    /// Updates an existing trip event in Google Calendar.
    /// </summary>
    /// <param name="googleAccessToken">Google OAuth access token.</param>
    /// <param name="trip">Updated trip details.</param>
    /// <returns>True if successful; false otherwise.</returns>
    Task<bool> UpdateTripEventAsync(string googleAccessToken, TripBucketItem trip);

    /// <summary>
    /// Deletes a trip event from Google Calendar.
    /// </summary>
    /// <param name="googleAccessToken">Google OAuth access token.</param>
    /// <param name="googleEventId">The Google Calendar Event ID to delete.</param>
    /// <returns>True if successful; false otherwise.</returns>
    Task<bool> DeleteTripEventAsync(string googleAccessToken, string googleEventId);
}
