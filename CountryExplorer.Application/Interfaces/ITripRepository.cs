using CountryExplorer.Domain.Entities;

namespace CountryExplorer.Application.Interfaces;

/// <summary>
/// Repository interface for TripBucketItem data access.
/// Enforces tenant security by requiring userId in all relevant methods.
/// </summary>
public interface ITripRepository
{
    /// <summary>
    /// Retrieves a trip by ID and verifies it belongs to the specified user.
    /// </summary>
    Task<TripBucketItem?> GetByIdAndUserAsync(int tripId, Guid userId);

    /// <summary>
    /// Retrieves all trips for a specific user with pagination support.
    /// </summary>
    Task<IEnumerable<TripBucketItem>> GetAllForUserAsync(Guid userId, int skip, int take);

    /// <summary>
    /// Adds a new trip to the database.
    /// </summary>
    Task<TripBucketItem> AddAsync(TripBucketItem trip);

    /// <summary>
    /// Updates an existing trip.
    /// </summary>
    Task<TripBucketItem> UpdateAsync(TripBucketItem trip);

    /// <summary>
    /// Soft-deletes a trip (marks as deleted without removing from DB).
    /// </summary>
    Task<bool> DeleteAsync(int tripId, Guid userId);
}
