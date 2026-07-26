using CountryExplorer.Application.DTOs.Trip;

namespace CountryExplorer.Application.DTOs.Auth;

public class UserFullProfileDto
{
    public UserProfileDto Profile { get; set; } = null!;
    public List<TripItemResponseDto> PlannedTrips { get; set; } = new();
    public List<TripItemResponseDto> VisitedTrips { get; set; } = new();
    public int TotalCountriesVisited => VisitedTrips
        .Select(t => t.CountryCode)
        .Distinct()
        .Count();
}