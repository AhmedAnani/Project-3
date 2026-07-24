using CountryExplorer.Application.DTOs.Attractions;

namespace CountryExplorer.Application.DTOs.Countries;

public class CountryDetailsDto
{
    public CountryDto Country { get; set; } = new();
    public List<TouristAttractionDto> Attractions { get; set; } = new();
}