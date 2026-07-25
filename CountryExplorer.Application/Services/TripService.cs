using AutoMapper;
using CountryExplorer.Application.DTOs;
using CountryExplorer.Application.DTOs.Trip;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.Interfaces.Repositories;
using CountryExplorer.Application.Interfaces.External;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Application.Services;

public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly ICountryApiService _countryApiService;
    private readonly IMapper _mapper;
    private readonly ILogger<TripService> _logger;

    public TripService(
        ITripRepository tripRepository,
        IGoogleCalendarService googleCalendarService,
        ICountryApiService countryApiService,
        IMapper mapper,
        ILogger<TripService> logger)
    {
        _tripRepository = tripRepository;
        _googleCalendarService = googleCalendarService;
        _countryApiService = countryApiService;
        _mapper = mapper;
        _logger = logger;
    }

    private async Task<string> ResolveCountryNameAsync(string countryCode)
    {
        try
        {
            var country = await _countryApiService.GetByCodeAsync(countryCode);
            if (country != null && !string.IsNullOrWhiteSpace(country.Name))
            {
                return country.Name;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve country name for code {CountryCode}.", countryCode);
        }
        return countryCode;
    }

    public async Task<TripItemResponseDto?> GetTripAsync(Guid userId, int tripId, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepository.GetByIdAndUserAsync(tripId, userId, cancellationToken);
        return trip is null ? null : _mapper.Map<TripItemResponseDto>(trip);
    }

    public async Task<PagedResult<TripItemResponseDto>> GetAllTripsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var pagedTrips = await _tripRepository.GetAllForUserAsync(userId, pageNumber, pageSize, cancellationToken);

        return new PagedResult<TripItemResponseDto>
        {
            Items = pagedTrips.Items.Select(_mapper.Map<TripItemResponseDto>).ToList(),
            TotalCount = pagedTrips.TotalCount,
            PageNumber = pagedTrips.PageNumber,
            PageSize = pagedTrips.PageSize
        };
    }

    public async Task<TripItemResponseDto> CreateTripAsync(Guid userId, TripItemCreateDto dto, string? googleAccessToken = null, CancellationToken cancellationToken = default)
    {
        var trip = _mapper.Map<TripBucketItem>(dto);
        trip.UserId = userId;
        trip.CountryName = await ResolveCountryNameAsync(dto.CountryCode);
        trip.Status = TripStatus.Planned;

        if (dto.SyncWithGoogleCalendar)
        {
            if (string.IsNullOrWhiteSpace(googleAccessToken))
            {
                throw new InvalidOperationException("Google Calendar sync was requested, but no Google OAuth access token was provided in the X-Google-Token header.");
            }

            var googleEventId = await _googleCalendarService.ScheduleTripEventAsync(
                googleAccessToken, trip.Title, trip.Notes, trip.StartDate, trip.EndDate, cancellationToken);

            if (!string.IsNullOrWhiteSpace(googleEventId))
            {
                trip.GoogleEventId = googleEventId;
            }
            else
            {
                _logger.LogWarning("Google Calendar event creation failed for new trip.");
            }
        }

        trip = await _tripRepository.AddAsync(trip, cancellationToken);
        return _mapper.Map<TripItemResponseDto>(trip);
    }

    public async Task<TripItemResponseDto> UpdateTripAsync(Guid userId, int tripId, TripItemUpdateDto dto, string? googleAccessToken = null, CancellationToken cancellationToken = default)
    {
        // Loaded with EF Core tracking enabled
        var trip = await _tripRepository.GetByIdAndUserForUpdateAsync(tripId, userId, cancellationToken)
            ?? throw new KeyNotFoundException($"Trip {tripId} was not found for the current user.");

        var previousGoogleEventId = trip.GoogleEventId;

        _mapper.Map(dto, trip);
        trip.CountryName = await ResolveCountryNameAsync(dto.CountryCode);
        trip.UpdatedAt = DateTime.UtcNow;

        if (!dto.SyncWithGoogleCalendar)
        {
            if (!string.IsNullOrWhiteSpace(previousGoogleEventId))
            {
                if (string.IsNullOrWhiteSpace(googleAccessToken))
                {
                    throw new InvalidOperationException("Cannot disable Google Calendar sync: an X-Google-Token header is required to remove the existing event from Google Calendar.");
                }

                var deleted = await _googleCalendarService.DeleteTripEventAsync(googleAccessToken, previousGoogleEventId, cancellationToken);
                if (!deleted)
                {
                    _logger.LogWarning("Failed to delete Google event {GoogleEventId} while disabling sync for trip {TripId}.", previousGoogleEventId, tripId);
                }

                trip.GoogleEventId = null;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(googleAccessToken))
            {
                throw new InvalidOperationException("Google Calendar sync is enabled, but no Google OAuth access token was provided in the X-Google-Token header.");
            }

            if (string.IsNullOrWhiteSpace(previousGoogleEventId))
            {
                var googleEventId = await _googleCalendarService.ScheduleTripEventAsync(
                    googleAccessToken, trip.Title, trip.Notes, trip.StartDate, trip.EndDate, cancellationToken);

                if (!string.IsNullOrWhiteSpace(googleEventId))
                {
                    trip.GoogleEventId = googleEventId;
                }
                else
                {
                    _logger.LogWarning("Google Calendar event creation failed while updating trip {TripId}.", tripId);
                }
            }
            else
            {
                var updated = await _googleCalendarService.UpdateTripEventAsync(
                    googleAccessToken, previousGoogleEventId, trip.Title, trip.Notes, trip.StartDate, trip.EndDate, cancellationToken);

                if (!updated)
                {
                    _logger.LogWarning("Failed to update Google event {GoogleEventId} for trip {TripId}.", previousGoogleEventId, tripId);
                }
            }
        }

        trip = await _tripRepository.UpdateAsync(trip, cancellationToken);
        return _mapper.Map<TripItemResponseDto>(trip);
    }

    public async Task<bool> DeleteTripAsync(Guid userId, int tripId, string? googleAccessToken = null, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepository.GetByIdAndUserAsync(tripId, userId, cancellationToken);
        if (trip is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(trip.GoogleEventId))
        {
            if (string.IsNullOrWhiteSpace(googleAccessToken))
            {
                throw new InvalidOperationException("Cannot delete trip: this trip is synced with Google Calendar, but no Google OAuth access token was provided in the X-Google-Token header.");
            }

            var deleted = await _googleCalendarService.DeleteTripEventAsync(googleAccessToken, trip.GoogleEventId, cancellationToken);
            if (!deleted)
            {
                _logger.LogWarning("Failed to delete Google event {GoogleEventId} while deleting trip {TripId}.", trip.GoogleEventId, tripId);
            }
        }

        return await _tripRepository.DeleteAsync(tripId, userId, cancellationToken);
    }
}
