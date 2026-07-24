using CountryExplorer.Application.DTOs.Budget;

namespace CountryExplorer.Application.Interfaces.Services;

public interface IExchangeRateService
{
    Task<BudgetEstimateDto> ConvertAsync(string toCurrencyCode, string fromCurrencyCode, decimal amount);
}