using System.Text.Json.Serialization;

namespace CountryExplorer.Infrastructure.ExternalServices.Models;

public class OpenTripMapPlaceResponse
{
    [JsonPropertyName("xid")]
    public string Xid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("kinds")]
    public string Kinds  { get; set; } = string.Empty;

    [JsonPropertyName("point")]
    public OpenTripMapPoint Point { get; set; } = new();

    [JsonPropertyName("dist")]
    public double? Dist { get; set; }
}

public class OpenTripMapPoint
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}