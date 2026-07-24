using CountryExplorer.Application.DTOs;

namespace CountryExplorer.Application.Interfaces;

/// <summary>
/// Service interface for orchestrating trip operations.
/// This is the strict entry point that Controllers should use.
/// Coordinates between ITripRepository and IGoogleCalendarService.
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Retrieves a trip by ID, ensuring it belongs to the specified user.
    /// </summary>
    Task<TripItemResponseDto?> GetTripAsync(Guid userId, int tripId);

    /// <summary>
    /// Retrieves all trips for a user with pagination support.
    /// </summary>
    Task<PagedResult<TripItemResponseDto>> GetAllTripsAsync(Guid userId, int pageNumber, int pageSize);

    /// <summary>
    /// Creates a new trip and optionally syncs with Google Calendar.
    /// </summary>
    /// <param name="userId">The ID of the user creating the trip.</param>
    /// <param name="dto">Trip creation data.</param>
    /// <param name="googleAccessToken">Google OAuth token (required if SyncWithGoogleCalendar is true).</param>
    /// <returns>The created trip as a response DTO.</returns>
    Task<TripItemResponseDto> CreateTripAsync(Guid userId, TripItemCreateDto dto, string? googleAccessToken = null);

    /// <summary>
    /// Updates an existing trip and optionally syncs changes with Google Calendar.
    /// </summary>
    /// <param name="userId">The ID of the user updating the trip.</param>
    /// <param name="tripId">The ID of the trip to update.</param>
    /// <param name="dto">Updated trip data.</param>
    /// <param name="googleAccessToken">Google OAuth token (required if SyncWithGoogleCalendar is true).</param>
    /// <returns>The updated trip as a response DTO.</returns>
    Task<TripItemResponseDto> UpdateTripAsync(Guid userId, int tripId, TripItemUpdateDto dto, string? googleAccessToken = null);

    /// <summary>
    /// Deletes a trip and removes it from Google Calendar if applicable.
    /// </summary>
    /// <param name="userId">The ID of the user deleting the trip.</param>
    /// <param name="tripId">The ID of the trip to delete.</param>
    /// <param name="googleAccessToken">Google OAuth token (required if the trip has a GoogleEventId).</param>
    /// <returns>True if deletion was successful; false otherwise.</returns>
    Task<bool> DeleteTripAsync(Guid userId, int tripId, string? googleAccessToken = null);
}
