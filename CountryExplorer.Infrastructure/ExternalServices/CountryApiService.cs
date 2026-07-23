using AutoMapper;
using CountryExplorer.Application.Exceptions;
using CountryExplorer.Application.DTOs.Countries;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CountryExplorer.Infrastructure.ExternalServices;

public class CountryApiService : ICountryApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CountryApiService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    public CountryApiService(
        HttpClient httpClient,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CountryApiService> logger)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CountryDto?> SearchByNameAsync(string name)
    {
        var cacheKey = $"country_name_{name.ToLowerInvariant()}";
        return await GetOrFetchAsync(cacheKey, $"names.common/{name}", name);
    }

    public async Task<CountryDto?> GetByCodeAsync(string code)
    {
        var cacheKey = $"country_code_{code.ToLowerInvariant()}";
        return await GetOrFetchAsync(cacheKey, $"codes.alpha_2/{code}", code);
    }

    private async Task<CountryDto?> GetOrFetchAsync(string cacheKey, string endpoint, string identifier)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new CountryNotFoundException(identifier);
            }

            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<RestCountriesEnvelope>();

            if (envelope is null || envelope.Data.Objects.Count == 0)
            {
                throw new CountryNotFoundException(identifier);
            }

            var dto = _mapper.Map<CountryDto>(envelope.Data.Objects.First());

            _cache.Set(cacheKey, dto, CacheDuration);

            return dto;
        }
        catch (CountryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "REST Countries API call failed for '{Identifier}'. Attempting cache fallback.", identifier);

            if (_cache.TryGetValue(cacheKey, out CountryDto? cachedDto))
            {
                return cachedDto;
            }

            throw new ExternalServiceUnavailableException("REST Countries API");
        }
    }
}