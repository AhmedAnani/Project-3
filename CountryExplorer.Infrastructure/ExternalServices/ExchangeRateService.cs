using CountryExplorer.Application.Exceptions;
using CountryExplorer.Application.DTOs.Budget;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CountryExplorer.Infrastructure.ExternalServices;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly string _apiKey;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public ExchangeRateService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<ExchangeRateService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _apiKey = configuration["ExchangeRate:ApiKey"]
            ?? throw new InvalidOperationException("ExchangeRate API key is missing from configuration.");
    }

    public async Task<BudgetEstimateDto> ConvertAsync(string toCurrencyCode, string fromCurrencyCode, decimal amount)
    {
        var cacheKey = $"exchange_rates_{toCurrencyCode.ToUpperInvariant()}";

        ExchangeRateResponse? rates;

        try
        {
            var url = $"{_apiKey}/latest/{toCurrencyCode}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            rates = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();

            if (rates is null || rates.Result != "success")
            {
                throw new ExternalServiceUnavailableException("ExchangeRate API");
            }

            _cache.Set(cacheKey, rates, CacheDuration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ExchangeRate API call failed for base '{Currency}'. Attempting cache fallback.", toCurrencyCode);

            if (!_cache.TryGetValue(cacheKey, out rates) || rates is null)
            {
                throw new ExternalServiceUnavailableException("ExchangeRate API");
            }
        }

        if (!rates.ConversionRates.TryGetValue(fromCurrencyCode.ToUpperInvariant(), out var rate))
        {
            throw new CurrencyNotSupportedException(fromCurrencyCode);
        }

        var convertedAmount = amount / rate;

        return new BudgetEstimateDto
        {
            OriginalAmount = amount,
            FromCurrency = fromCurrencyCode.ToUpperInvariant(),
            ConvertedAmount = Math.Round(convertedAmount, 2),
            ToCurrency = toCurrencyCode.ToUpperInvariant(),
            ExchangeRate = rate,
            LastUpdatedUtc = DateTime.UtcNow
        };
    }
}