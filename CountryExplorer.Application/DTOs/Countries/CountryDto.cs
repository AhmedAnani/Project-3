namespace CountryExplorer.Application.DTOs.Countries;

public class CountryDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OfficialName { get; set; } = string.Empty;
    public string Capital { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Subregion { get; set; } = string.Empty;
    public List<CurrencyDto> Currencies { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public string FlagUrl { get; set; } = string.Empty;
    public long Population { get; set; }
    public double? CapitalLatitude { get; set; }
    public double? CapitalLongitude { get; set; }
}