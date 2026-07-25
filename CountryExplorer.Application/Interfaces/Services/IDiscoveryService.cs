using CountryExplorer.Application.DTOs;
using CountryExplorer.Application.DTOs.Trip;
using CountryExplorer.Application.Dtos.Discovery;
using CountryExplorer.Application.DTOs.Discovery;

namespace CountryExplorer.Application.Interfaces.Services;

/// <summary>
/// Service interface for the destination discovery/recommendation engine.
/// </summary>
public interface IDiscoveryService
{
    /// <summary>
    /// Returns ranked destination recommendations based on the user's intent and preferences.
    /// </summary>
    Task<List<ScoreBreakdownDto>> GetRecommendationsAsync(
        DiscoveryRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a recommended destination to the user's trip bucket list.
    /// </summary>
    Task<TripItemResponseDto> SaveToTripsAsync(
        Guid userId,
        int destinationId,
        SaveDiscoveryRequestDto request,
        CancellationToken cancellationToken = default);
}
