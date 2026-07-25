namespace CountryExplorer.Domain.Enums;

/// <summary>
/// Bitwise flags describing a destination's characteristics.
/// Combine with | operator: e.g. IsCoastal | IsHistoric | IsWarmClimate.
/// </summary>
[Flags]
public enum VibeTag
{
    None          = 0,
    IsCoastal     = 1,
    HasMountains  = 2,
    IsHistoric    = 4,
    IsUrban       = 8,
    IsWarmClimate = 16,
    IsColdClimate = 32
}
