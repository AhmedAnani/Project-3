using CountryExplorer.Application.DTOs.Countries;
using CountryExplorer.Application.Interfaces.Services;

namespace CountryExplorer.Application.Services;

public class CountryExplorerService : ICountryExplorerService
{
    private readonly ICountryApiService _countryApiService;
    private readonly ITouristAttractionService _touristAttractionService;

    public CountryExplorerService(
        ICountryApiService countryApiService,
        ITouristAttractionService touristAttractionService)
    {
        _countryApiService = countryApiService;
        _touristAttractionService = touristAttractionService;
    }

    public async Task<CountryDetailsDto?> GetCountryDetailsAsync(string name)
    {
        var country = await _countryApiService.SearchByNameAsync(name);

        var attractions = new List<Application.DTOs.Attractions.TouristAttractionDto>();

        if (country.CapitalLatitude.HasValue && country.CapitalLongitude.HasValue)
        {
            attractions = await _touristAttractionService.GetAttractionsByCoordinatesAsync(
                country.CapitalLatitude.Value,
                country.CapitalLongitude.Value);
        }

        return new CountryDetailsDto
        {
            Country = country,
            Attractions = attractions
        };
    }
}