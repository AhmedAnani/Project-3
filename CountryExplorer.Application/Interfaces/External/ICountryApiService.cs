using CountryExplorer.Application.DTOs.Countries;

namespace CountryExplorer.Application.Interfaces.External;

public interface ICountryApiService
{
    Task<CountryDto?> SearchByNameAsync(string name);
    Task<CountryDto?> GetByCodeAsync(string code);
}