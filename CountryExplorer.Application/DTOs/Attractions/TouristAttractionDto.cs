namespace CountryExplorer.Application.DTOs.Attractions;

public class TouristAttractionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DistanceFromCenterMeters { get; set; }
}