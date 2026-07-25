using CountryExplorer.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CountryExplorer.Application.DTOs.Discovery;

/// <summary>
/// Input DTO for the destination recommendation engine.
/// </summary>
public class DiscoveryRequestDto
{
    /// <summary>
    /// The user's primary trip intent — determines scoring weight profile.
    /// </summary>
    [Required]
    public TripPurpose Purpose { get; set; }

    /// <summary>
    /// Maximum acceptable cost-of-living index (0–100 scale).
    /// Destinations above this threshold are disqualified.
    /// Default: 80.
    /// </summary>
    [Range(0, 100)]
    public int MaxBudget { get; set; } = 80;

    /// <summary>
    /// Optional vibe preferences to boost matching score.
    /// Use bitwise OR for multiple: e.g. IsCoastal | IsWarmClimate.
    /// </summary>
    public VibeTag? PreferredTags { get; set; }

    /// <summary>
    /// Number of top results to return. Default: 5.
    /// </summary>
    [Range(1, 50)]
    public int TopN { get; set; } = 5;
}
