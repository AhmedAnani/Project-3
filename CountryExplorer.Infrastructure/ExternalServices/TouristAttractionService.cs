using AutoMapper;
using CountryExplorer.Application.Exceptions;
using CountryExplorer.Application.DTOs.Attractions;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CountryExplorer.Infrastructure.ExternalServices;

public class TouristAttractionService : ITouristAttractionService
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TouristAttractionService> _logger;
    private readonly string _apiKey;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);
    private const int MaxResults = 10;
    private const int SearchRadiusMeters = 15000;

    public TouristAttractionService(
        HttpClient httpClient,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<TouristAttractionService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
        _apiKey = configuration["OpenTripMap:ApiKey"]
            ?? throw new InvalidOperationException("OpenTripMap API key is missing from configuration.");
    }

    public async Task<List<TouristAttractionDto>> GetAttractionsByCoordinatesAsync(double latitude, double longitude)
    {
        var cacheKey = $"attractions_{latitude}_{longitude}";

        try
        {
            var url = $"radius?radius={SearchRadiusMeters}&lon={longitude}&lat={latitude}" +
                       $"&kinds=interesting_places&limit={MaxResults}&format=json&apikey={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var apiResult = await response.Content.ReadFromJsonAsync<List<OpenTripMapPlaceResponse>>();

            var dtoList = _mapper.Map<List<TouristAttractionDto>>(apiResult ?? new List<OpenTripMapPlaceResponse>());

            _cache.Set(cacheKey, dtoList, CacheDuration);

            return dtoList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenTripMap radius search failed for ({Lat}, {Lon}). Attempting cache fallback.", latitude, longitude);

            if (_cache.TryGetValue(cacheKey, out List<TouristAttractionDto>? cachedList))
            {
                return cachedList!;
            }

            return new List<TouristAttractionDto>();
        }
    }

    public async Task<TouristAttractionDetailsDto?> GetAttractionDetailsAsync(string xid)
    {
        var cacheKey = $"attraction_details_{xid}";

        try
        {
            var url = $"xid/{xid}?apikey={_apiKey}";

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new AttractionNotFoundException(xid);
            }

            response.EnsureSuccessStatusCode();

            var apiResult = await response.Content.ReadFromJsonAsync<OpenTripMapPlaceDetailsResponse>();

            if (apiResult is null)
            {
                throw new AttractionNotFoundException(xid);
            }

            var dto = _mapper.Map<TouristAttractionDetailsDto>(apiResult);

            _cache.Set(cacheKey, dto, CacheDuration);

            return dto;
        }
        catch (AttractionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenTripMap details call failed for xid '{Xid}'. Attempting cache fallback.", xid);

            if (_cache.TryGetValue(cacheKey, out TouristAttractionDetailsDto? cachedDto))
            {
                return cachedDto;
            }

            throw new ExternalServiceUnavailableException("OpenTripMap API");
        }
    }
}