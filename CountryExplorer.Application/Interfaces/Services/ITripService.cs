using CountryExplorer.Application.DTOs.Trip;
using CountryExplorer.Application.DTOs;

namespace CountryExplorer.Application.Interfaces.Services;

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
    Task<TripItemResponseDto?> GetTripAsync(Guid userId, int tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all trips for a user with pagination support.
    /// </summary>
    Task<PagedResult<TripItemResponseDto>> GetAllTripsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new trip and optionally syncs with Google Calendar.
    /// </summary>
    Task<TripItemResponseDto> CreateTripAsync(Guid userId, TripItemCreateDto dto, string? googleAccessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing trip and optionally syncs changes with Google Calendar.
    /// </summary>
    Task<TripItemResponseDto> UpdateTripAsync(Guid userId, int tripId, TripItemUpdateDto dto, string? googleAccessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a trip and removes it from Google Calendar if applicable.
    /// </summary>
    Task<bool> DeleteTripAsync(Guid userId, int tripId, string? googleAccessToken = null, CancellationToken cancellationToken = default);
}
