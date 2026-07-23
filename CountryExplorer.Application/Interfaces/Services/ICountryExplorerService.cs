using CountryExplorer.Application.DTOs.Countries;

namespace CountryExplorer.Application.Interfaces.Services;

public interface ICountryExplorerService
{
    Task<CountryDetailsDto?> GetCountryDetailsAsync(string name);
}