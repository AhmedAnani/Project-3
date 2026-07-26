using CountryExplorer.Application.DTOs.Auth;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Application.Services;

public class UserProfileService : IUserProfileService
{
    // Trips are typically few per user; a large single page keeps this simple
    // without adding a new "get all, unpaginated" method to ITripService.
    private const int MaxTripsToAggregate = 500;

    private readonly IUserService _userService;
    private readonly ITripService _tripService;

    public UserProfileService(IUserService userService, ITripService tripService)
    {
        _userService = userService;
        _tripService = tripService;
    }

    public async Task<UserFullProfileDto?> GetFullProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _userService.GetCurrentUserAsync(userId, ct);
        if (profile is null)
        {
            return null;
        }

        var allTrips = await _tripService.GetAllTripsAsync(userId, pageNumber: 1, pageSize: MaxTripsToAggregate, ct);

        return new UserFullProfileDto
        {
            Profile = profile,
            PlannedTrips = allTrips.Items.Where(t => t.Status == TripStatus.Planned).ToList(),
            VisitedTrips = allTrips.Items.Where(t => t.Status == TripStatus.Visited).ToList()
        };
    }
}