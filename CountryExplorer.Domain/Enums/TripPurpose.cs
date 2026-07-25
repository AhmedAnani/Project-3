namespace CountryExplorer.Domain.Enums;

/// <summary>
/// Represents the user's primary intent for their trip.
/// Used by the recommendation engine to weight scoring criteria.
/// </summary>
public enum TripPurpose
{
    Workation,
    Adventure,
    Vacation,
    CultureHistory
}
