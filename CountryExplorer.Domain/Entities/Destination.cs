using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Domain.Entities;

/// <summary>
/// A travel destination with quantified metrics for the recommendation engine.
/// All index scores are on a 0–100 scale.
/// </summary>
public class Destination
{
    public int Id { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code (e.g. "PT", "JP").</summary>
    public string CountryCode { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    /// <summary>Optional one-liner for display purposes.</summary>
    public string? Description { get; set; }

    /// <summary>0 = no connectivity, 100 = world-class fibre.</summary>
    public int InternetQualityScore { get; set; }

    /// <summary>0 = dangerous, 100 = safest in the world.</summary>
    public int SafetyIndex { get; set; }

    /// <summary>0 = extremely cheap, 100 = extremely expensive.</summary>
    public int CostOfLivingIndex { get; set; }

    /// <summary>Bitwise combination of <see cref="VibeTag"/> flags.</summary>
    public VibeTag Tags { get; set; } = VibeTag.None;
}
