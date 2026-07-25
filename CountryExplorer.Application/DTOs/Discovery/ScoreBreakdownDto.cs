namespace CountryExplorer.Application.Dtos.Discovery;

/// <summary>
/// Immutable breakdown of how a destination scored against a user's intent.
/// If <see cref="IsDisqualified"/> is true, the destination was eliminated by a
/// hard dealbreaker (e.g., budget ceiling) before the weighted average ran.
/// </summary>
public sealed record ScoreBreakdownDto
{
    public required int DestinationId { get; init; }
    public required string CityName { get; init; }
    public required string CountryCode { get; init; }

    /// <summary>Score derived from climate VibeTag match (0–100).</summary>
    public double ClimateScore { get; init; }

    /// <summary>Composite of InternetQualityScore + SafetyIndex (0–100).</summary>
    public double InfrastructureScore { get; init; }

    /// <summary>How well the destination's tags match the requested tags (0–100).</summary>
    public double VibeMatchScore { get; init; }

    /// <summary>Inverse of CostOfLivingIndex — cheaper destinations score higher (0–100).</summary>
    public double CostScore { get; init; }

    /// <summary>Final weighted composite across all dimensions.</summary>
    public double TotalWeightedScore { get; init; }

    /// <summary>True if the destination was eliminated by a hard dealbreaker before scoring.</summary>
    public bool IsDisqualified { get; init; }

    /// <summary>Human-readable reason for disqualification (null if qualified).</summary>
    public string? DisqualificationReason { get; init; }
}
