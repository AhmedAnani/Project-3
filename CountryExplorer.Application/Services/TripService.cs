using AutoMapper;
using CountryExplorer.Application.DTOs;
using CountryExplorer.Application.Interfaces;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Application.Services;

public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IMapper _mapper;
    private readonly ILogger<TripService> _logger;

    public TripService(
        ITripRepository tripRepository,
        IGoogleCalendarService googleCalendarService,
        IMapper mapper,
        ILogger<TripService> logger)
    {
        _tripRepository = tripRepository;
        _googleCalendarService = googleCalendarService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TripItemResponseDto?> GetTripAsync(Guid userId, int tripId)
    {
        var trip = await _tripRepository.GetByIdAndUserAsync(tripId, userId);
        return trip is null ? null : _mapper.Map<TripItemResponseDto>(trip);
    }

    public async Task<PagedResult<TripItemResponseDto>> GetAllTripsAsync(Guid userId, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var pagedTrips = await _tripRepository.GetAllForUserAsync(userId, pageNumber, pageSize);

        return new PagedResult<TripItemResponseDto>
        {
            Items = pagedTrips.Items.Select(_mapper.Map<TripItemResponseDto>).ToList(),
            TotalCount = pagedTrips.TotalCount,
            PageNumber = pagedTrips.PageNumber,
            PageSize = pagedTrips.PageSize
        };
    }

    public async Task<TripItemResponseDto> CreateTripAsync(Guid userId, TripItemCreateDto dto, string? googleAccessToken = null)
    {
        var trip = _mapper.Map<TripBucketItem>(dto);
        trip.UserId = userId;
        trip.CountryName = "Pending Lookup";
        trip.Status = TripStatus.Planned;

        trip = await _tripRepository.AddAsync(trip);

        if (dto.SyncWithGoogleCalendar)
        {
            if (!string.IsNullOrWhiteSpace(googleAccessToken))
            {
                var googleEventId = await _googleCalendarService.ScheduleTripEventAsync(googleAccessToken, trip);
                if (!string.IsNullOrWhiteSpace(googleEventId))
                {
                    trip.GoogleEventId = googleEventId;
                    trip = await _tripRepository.UpdateAsync(trip);
                }
                else
                {
                    _logger.LogWarning("Google Calendar event creation failed for trip {TripId}.", trip.Id);
                }
            }
            else
            {
                _logger.LogWarning("Google sync requested for trip creation but no access token was provided for user {UserId}.", userId);
            }
        }

        return _mapper.Map<TripItemResponseDto>(trip);
    }

    public async Task<TripItemResponseDto> UpdateTripAsync(Guid userId, int tripId, TripItemUpdateDto dto, string? googleAccessToken = null)
    {
        var trip = await _tripRepository.GetByIdAndUserAsync(tripId, userId)
            ?? throw new KeyNotFoundException($"Trip {tripId} was not found for the current user.");

        var previousGoogleEventId = trip.GoogleEventId;

        _mapper.Map(dto, trip);
        trip.CountryName = "Pending Lookup";
        trip.UpdatedAt = DateTime.UtcNow;

        trip = await _tripRepository.UpdateAsync(trip);

        if (!dto.SyncWithGoogleCalendar)
        {
            if (!string.IsNullOrWhiteSpace(previousGoogleEventId))
            {
                if (!string.IsNullOrWhiteSpace(googleAccessToken))
                {
                    var deleted = await _googleCalendarService.DeleteTripEventAsync(googleAccessToken, previousGoogleEventId);
                    if (!deleted)
                    {
                        _logger.LogWarning("Failed to delete Google event {GoogleEventId} while disabling sync for trip {TripId}.", previousGoogleEventId, tripId);
                    }
                }
                else
                {
                    _logger.LogWarning("Trip {TripId} has an existing Google event but no access token was provided to delete it.", tripId);
                }

                trip.GoogleEventId = null;
                trip = await _tripRepository.UpdateAsync(trip);
            }
        }
        else if (!string.IsNullOrWhiteSpace(googleAccessToken))
        {
            if (string.IsNullOrWhiteSpace(previousGoogleEventId))
            {
                var googleEventId = await _googleCalendarService.ScheduleTripEventAsync(googleAccessToken, trip);
                if (!string.IsNullOrWhiteSpace(googleEventId))
                {
                    trip.GoogleEventId = googleEventId;
                    trip = await _tripRepository.UpdateAsync(trip);
                }
                else
                {
                    _logger.LogWarning("Google Calendar event creation failed while updating trip {TripId}.", tripId);
                }
            }
            else
            {
                var updated = await _googleCalendarService.UpdateTripEventAsync(googleAccessToken, trip);
                if (!updated)
                {
                    _logger.LogWarning("Failed to update Google event {GoogleEventId} for trip {TripId}.", previousGoogleEventId, tripId);
                }
            }
        }
        else
        {
            _logger.LogWarning("Google sync requested for trip update but no access token was provided for trip {TripId}.", tripId);
        }

        return _mapper.Map<TripItemResponseDto>(trip);
    }

    public async Task<bool> DeleteTripAsync(Guid userId, int tripId, string? googleAccessToken = null)
    {
        var trip = await _tripRepository.GetByIdAndUserAsync(tripId, userId);
        if (trip is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(trip.GoogleEventId))
        {
            if (!string.IsNullOrWhiteSpace(googleAccessToken))
            {
                var deleted = await _googleCalendarService.DeleteTripEventAsync(googleAccessToken, trip.GoogleEventId);
                if (!deleted)
                {
                    _logger.LogWarning("Failed to delete Google event {GoogleEventId} while deleting trip {TripId}.", trip.GoogleEventId, tripId);
                }
            }
            else
            {
                _logger.LogWarning("Trip {TripId} has a Google event but no access token was provided to delete it.", tripId);
            }
        }

        return await _tripRepository.DeleteAsync(tripId, userId);
    }
}
