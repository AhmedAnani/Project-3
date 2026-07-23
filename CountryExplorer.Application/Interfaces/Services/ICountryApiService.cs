using CountryExplorer.Application.DTOs.Countries;

namespace CountryExplorer.Application.Interfaces.Services;

public interface ICountryApiService
{
    Task<CountryDto> SearchByNameAsync(string name);
    Task<CountryDto> GetByCodeAsync(string code);
}