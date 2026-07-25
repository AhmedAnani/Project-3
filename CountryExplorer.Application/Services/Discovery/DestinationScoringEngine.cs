using CountryExplorer.Application.Dtos.Discovery;
using CountryExplorer.Application.DTOs.Discovery;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Application.Services.Discovery;

/// <summary>
/// Pure static scoring engine — no dependencies, fully unit-testable.
/// Scores a single destination against a user's discovery request.
/// </summary>
public static class DestinationScoringEngine
{
    // ── Weight profiles per TripPurpose ────────────────────────────────
    //                                      Infra  Vibe   Cost   Climate
    private static readonly Dictionary<TripPurpose, (double Infra, double Vibe, double Cost, double Climate)> WeightProfiles = new()
    {
        [TripPurpose.Workation]      = (0.40, 0.15, 0.30, 0.15),
        [TripPurpose.Adventure]      = (0.15, 0.35, 0.25, 0.25),
        [TripPurpose.Vacation]       = (0.10, 0.30, 0.25, 0.35),
        [TripPurpose.CultureHistory] = (0.20, 0.40, 0.25, 0.15),
    };

    // ── Dealbreaker thresholds ────────────────────────────────────────
    private const int MinInternetForWorkation = 40;

    /// <summary>
    /// Scores a single destination against the user's request.
    /// Dealbreakers are checked first — if triggered, scoring is skipped.
    /// </summary>
    public static ScoreBreakdownDto Score(Destination destination, DiscoveryRequestDto request)
    {
        // ── Step 1: Dealbreaker checks (early exit) ───────────────────
        var (isDisqualified, reason) = CheckDealbreakers(destination, request);

        if (isDisqualified)
        {
            return new ScoreBreakdownDto
            {
                DestinationId = destination.Id,
                CityName = destination.CityName,
                CountryCode = destination.CountryCode,
                ClimateScore = 0,
                InfrastructureScore = 0,
                VibeMatchScore = 0,
                CostScore = 0,
                TotalWeightedScore = 0,
                IsDisqualified = true,
                DisqualificationReason = reason
            };
        }

        // ── Step 2: Compute individual dimension scores ───────────────
        double climateScore = ComputeClimateScore(destination.Tags);
        double infraScore = ComputeInfrastructureScore(destination);
        double vibeScore = ComputeVibeMatchScore(destination.Tags, request.PreferredTags);
        double costScore = ComputeCostScore(destination.CostOfLivingIndex);

        // ── Step 3: Apply weighted average ────────────────────────────
        var weights = WeightProfiles[request.Purpose];
        double total = (infraScore * weights.Infra)
                     + (vibeScore * weights.Vibe)
                     + (costScore * weights.Cost)
                     + (climateScore * weights.Climate);

        return new ScoreBreakdownDto
        {
            DestinationId = destination.Id,
            CityName = destination.CityName,
            CountryCode = destination.CountryCode,
            ClimateScore = Math.Round(climateScore, 2),
            InfrastructureScore = Math.Round(infraScore, 2),
            VibeMatchScore = Math.Round(vibeScore, 2),
            CostScore = Math.Round(costScore, 2),
            TotalWeightedScore = Math.Round(total, 2),
            IsDisqualified = false,
            DisqualificationReason = null
        };
    }

    // ── Dealbreaker logic ─────────────────────────────────────────────

    private static (bool IsDisqualified, string? Reason) CheckDealbreakers(
        Destination destination, DiscoveryRequestDto request)
    {
        // Budget ceiling
        if (destination.CostOfLivingIndex > request.MaxBudget)
        {
            return (true,
                $"Cost of living ({destination.CostOfLivingIndex}) exceeds budget ceiling ({request.MaxBudget})");
        }

        // Workation requires minimum internet quality
        if (request.Purpose == TripPurpose.Workation
            && destination.InternetQualityScore < MinInternetForWorkation)
        {
            return (true,
                $"Internet quality ({destination.InternetQualityScore}) too low for remote work (minimum: {MinInternetForWorkation})");
        }

        return (false, null);
    }

    // ── Dimension scoring ─────────────────────────────────────────────

    /// <summary>
    /// Climate score based on VibeTag climate flags.
    /// Warm → 80, Cold → 60, Neither → 50 (neutral baseline).
    /// </summary>
    private static double ComputeClimateScore(VibeTag tags)
    {
        if (tags.HasFlag(VibeTag.IsWarmClimate)) return 80;
        if (tags.HasFlag(VibeTag.IsColdClimate)) return 60;
        return 50; // neutral
    }

    /// <summary>
    /// Infrastructure = average of internet quality and safety (0–100).
    /// </summary>
    private static double ComputeInfrastructureScore(Destination destination)
    {
        return (destination.InternetQualityScore + destination.SafetyIndex) / 2.0;
    }

    /// <summary>
    /// Vibe match = percentage of requested tags that the destination has.
    /// If no tags were requested, returns neutral baseline of 50.
    /// </summary>
    private static double ComputeVibeMatchScore(VibeTag destinationTags, VibeTag? requestedTags)
    {
        if (requestedTags is null || requestedTags == VibeTag.None)
            return 50; // neutral — no preference expressed

        var requested = requestedTags.Value;

        // Count how many individual flags were requested
        int totalRequested = CountFlags(requested);
        if (totalRequested == 0) return 50;

        // Count how many of those flags the destination actually has
        int matched = CountFlags(destinationTags & requested);

        return (matched / (double)totalRequested) * 100.0;
    }

    /// <summary>
    /// Cost score = inverse of cost index. Cheaper destinations score higher.
    /// </summary>
    private static double ComputeCostScore(int costOfLivingIndex)
    {
        return 100 - costOfLivingIndex;
    }

    /// <summary>
    /// Counts the number of set bits (individual flags) in a VibeTag value.
    /// </summary>
    private static int CountFlags(VibeTag tags)
    {
        int value = (int)tags;
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }
        return count;
    }
}
