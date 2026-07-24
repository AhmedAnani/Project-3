using CountryExplorer.Application.DTOs;
using CountryExplorer.Domain.Entities;

namespace CountryExplorer.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for TripBucketItem data access.
/// Enforces tenant security by requiring userId in all relevant methods.
/// </summary>
public interface ITripRepository
{
    /// <summary>
    /// Retrieves an untracked trip by ID for read-only scenarios.
    /// </summary>
    Task<TripBucketItem?> GetByIdAndUserAsync(int tripId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracked trip by ID for update scenarios, enabling EF Core change tracking.
    /// </summary>
    Task<TripBucketItem?> GetByIdAndUserForUpdateAsync(int tripId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all trips for a specific user with pagination support.
    /// </summary>
    Task<PagedResult<TripBucketItem>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new trip to the database.
    /// </summary>
    Task<TripBucketItem> AddAsync(TripBucketItem trip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing trip.
    /// </summary>
    Task<TripBucketItem> UpdateAsync(TripBucketItem trip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a trip (marks as deleted without removing from DB).
    /// </summary>
    Task<bool> DeleteAsync(int tripId, Guid userId, CancellationToken cancellationToken = default);
}
