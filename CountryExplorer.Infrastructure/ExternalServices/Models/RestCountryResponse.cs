using System.Text.Json.Serialization;

namespace CountryExplorer.Infrastructure.ExternalServices.Models;

public class RestCountriesEnvelope
{
    [JsonPropertyName("data")]
    public RestCountriesData Data { get; set; } = new();
}

public class RestCountriesData
{
    [JsonPropertyName("objects")]
    public List<RestCountryResponse> Objects { get; set; } = new();
}

public class RestCountryResponse
{
    [JsonPropertyName("names")]
    public RestCountryNames Names { get; set; } = new();

    [JsonPropertyName("codes")]
    public RestCountryCodes Codes { get; set; } = new();

    [JsonPropertyName("capitals")]
    public List<RestCountryCapital> Capitals { get; set; } = new();

    [JsonPropertyName("flag")]
    public RestCountryFlag Flag { get; set; } = new();

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("subregion")]
    public string Subregion { get; set; } = string.Empty;

    [JsonPropertyName("currencies")]
    public List<RestCountryCurrency> Currencies { get; set; } = new();

    [JsonPropertyName("languages")]
    public List<RestCountryLanguage> Languages { get; set; } = new();

    [JsonPropertyName("population")]
    public long Population { get; set; }
}

public class RestCountryNames
{
    [JsonPropertyName("common")]
    public string Common { get; set; } = string.Empty;

    [JsonPropertyName("official")]
    public string Official { get; set; } = string.Empty;
}

public class RestCountryCodes
{
    [JsonPropertyName("alpha_2")]
    public string Alpha2 { get; set; } = string.Empty;

    [JsonPropertyName("alpha_3")]
    public string Alpha3 { get; set; } = string.Empty;
}

public class RestCountryCapital
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("coordinates")]
    public RestCountryCoordinates Coordinates { get; set; } = new();
}

public class RestCountryCoordinates
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}

public class RestCountryFlag
{
    [JsonPropertyName("url_png")]
    public string UrlPng { get; set; } = string.Empty;
}

public class RestCountryCurrency
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
}

public class RestCountryLanguage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}