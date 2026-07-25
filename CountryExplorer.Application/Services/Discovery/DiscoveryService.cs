using CountryExplorer.Application.DTOs;
using CountryExplorer.Application.DTOs.Trip;
using CountryExplorer.Application.DTOs.Discovery;
using CountryExplorer.Application.Dtos.Discovery;
using CountryExplorer.Application.Interfaces.Repositories;
using CountryExplorer.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Application.Services.Discovery;

/// <summary>
/// Orchestrates the destination recommendation pipeline:
/// load destinations → score each → sort → return top N.
/// </summary>
public class DiscoveryService : IDiscoveryService
{
    private readonly IDestinationRepository _repository;
    private readonly ITripService _tripService;
    private readonly ILogger<DiscoveryService> _logger;

    public DiscoveryService(
        IDestinationRepository repository,
        ITripService tripService,
        ILogger<DiscoveryService> logger)
    {
        _repository = repository;
        _tripService = tripService;
        _logger = logger;
    }

    public async Task<List<ScoreBreakdownDto>> GetRecommendationsAsync(
        DiscoveryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var destinations = await _repository.GetAllAsync(cancellationToken);

        _logger.LogInformation(
            "Scoring {Count} destinations for purpose {Purpose} with budget ceiling {MaxBudget}",
            destinations.Count, request.Purpose, request.MaxBudget);

        // Score every destination
        var scored = destinations
            .Select(d => DestinationScoringEngine.Score(d, request))
            .ToList();

        // Sort: qualified first (highest score), disqualified appended at the end
        var ranked = scored
            .OrderBy(s => s.IsDisqualified)
            .ThenByDescending(s => s.TotalWeightedScore)
            .Take(request.TopN)
            .ToList();

        var qualifiedCount = ranked.Count(s => !s.IsDisqualified);
        var disqualifiedCount = ranked.Count(s => s.IsDisqualified);

        _logger.LogInformation(
            "Returning {TopN} results: {Qualified} qualified, {Disqualified} disqualified",
            ranked.Count, qualifiedCount, disqualifiedCount);

        return ranked;
    }

    public async Task<TripItemResponseDto> SaveToTripsAsync(
        Guid userId,
        int destinationId,
        SaveDiscoveryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch destination
        var destinations = await _repository.GetAllAsync(cancellationToken);
        var destination = destinations.FirstOrDefault(d => d.Id == destinationId);

        if (destination == null)
        {
            throw new KeyNotFoundException($"Destination with ID {destinationId} not found.");
        }

        // 2. Set dates based on input or defaults
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(1);
        var endDate = request.EndDate ?? startDate.AddDays(7);

        // 3. Create Trip payload
        var tripCreateDto = new TripItemCreateDto
        {
            Title = $"Trip to {destination.CityName}",
            CountryCode = destination.CountryCode,
            StartDate = startDate,
            EndDate = endDate,
            Notes = "Saved from Discovery Recommendations.",
            SyncWithGoogleCalendar = false
        };

        // 4. Save to User's Bucket List
        _logger.LogInformation("Saving discovered destination {CityName} to trips for User {UserId}", destination.CityName, userId);

        return await _tripService.CreateTripAsync(userId, tripCreateDto, null, cancellationToken);
    }
}
