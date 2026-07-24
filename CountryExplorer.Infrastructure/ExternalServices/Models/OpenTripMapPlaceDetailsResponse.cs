using System.Text.Json.Serialization;

namespace CountryExplorer.Infrastructure.ExternalServices.Models;

public class OpenTripMapPlaceDetailsResponse
{
    [JsonPropertyName("xid")]
    public string Xid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("kinds")]
    public string Kinds { get; set; } = string.Empty;

    [JsonPropertyName("point")]
    public OpenTripMapPoint Point { get; set; } = new();

    [JsonPropertyName("preview")]
    public OpenTripMapPreview? Preview { get; set; }

    [JsonPropertyName("wikipedia_extracts")]
    public OpenTripMapWikipediaExtracts? WikipediaExtracts { get; set; }
}

public class OpenTripMapPreview
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

public class OpenTripMapWikipediaExtracts
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}