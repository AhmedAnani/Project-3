using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.Interfaces.External;
using Microsoft.AspNetCore.Mvc;

namespace CountryExplorer.Api.Controllers;

[ApiController]
[Route("api/countries")]
public class CountriesController : ControllerBase
{
    private readonly ICountryExplorerService _countryExplorerService;
    private readonly ITouristAttractionService _touristAttractionService;
    private readonly IExchangeRateService _exchangeRateService;

    public CountriesController(
        ICountryExplorerService countryExplorerService,
        ITouristAttractionService touristAttractionService,
        IExchangeRateService exchangeRateService)
    {
        _countryExplorerService = countryExplorerService;
        _touristAttractionService = touristAttractionService;
        _exchangeRateService = exchangeRateService;
    }

    /// <summary>
    /// Retrieves detailed information about a country by its name, along with the top tourist attractions
    /// near its capital city, fetched live from REST Countries and OpenTripMap
    /// </summary>
    /// <param name="name">The name of the country to retrieve details for</param>
    /// <returns>The country's data and a list of nearby tourist attractions</returns>
    /// <response code="200">Returns the country details and nearby tourist attractions</response>
    /// <response code="400">The provided country name was empty or whitespace</response>
    /// <response code="404">No country was found for the provided name</response>

    [HttpGet("{name}")]
    public async Task<IActionResult> GetCountryDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Country name must not be empty.");
        }

        var result = await _countryExplorerService.GetCountryDetailsAsync(name);

        return Ok(result);
    }


    /// <summary>
    /// Retrieves full details (description and image) for a single tourist attraction by its OpenTripMap identifier (xid).
    /// </summary>
    /// <param name="xid">The OpenTripMap unique identifier for the attraction, typically obtained from the attractions
    /// list returned by <see cref="GetCountryDetails"/>.</param>
    /// <returns>The attraction's name, categories, description, image URL, and coordinates.</returns>
    /// <response code="200">Attraction found; returns its full details.</response>
    /// <response code="400">The provided xid was empty or whitespace.</response>
    /// <response code="404">No attraction was found for the provided xid.</response>
    [HttpGet("/api/attractions/{xid}")]
    public async Task<IActionResult> GetAttractionDetails(string xid)
    {
        if (string.IsNullOrWhiteSpace(xid))
        {
            return BadRequest("Attraction id must not be empty.");
        }

        var result = await _touristAttractionService.GetAttractionDetailsAsync(xid);

        return Ok(result);
    }

    /// <summary>
    /// Estimates a travel budget by converting a given amount from the user's currency to the destination
    /// country's official currency, using the latest available exchange rates (refreshed every 24 hours).
    /// </summary>
    /// <param name="name">The common name of the destination country Its official currency
    /// is resolved automatically and used as the conversion target.</param>
    /// <param name="amount">The amount of money to convert, expressed in <paramref name="fromCurrency"/>. Must be greater than zero.</param>
    /// <param name="fromCurrency">The ISO 4217 currency code the amount is currently in (e.g. "EGP", "USD", "SAR").</param>
    /// <returns>The original amount, the converted amount in the destination currency, the exchange rate used,
    /// and the timestamp of the last rate update.</returns>
    /// <response code="200">Conversion successful.</response>
    /// <response code="400">Missing/invalid parameters, or the provided currency code is not supported.</response>
    /// <response code="404">No country matches the provided name.</response>
    [HttpGet("{name}/budget-estimate")]
    public async Task<IActionResult> GetBudgetEstimate(string name, [FromQuery] decimal amount, [FromQuery] string fromCurrency)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(fromCurrency) || amount <= 0)
        {
            return BadRequest("Country name, fromCurrency, and a positive amount are required.");
        }

        var country = await _countryExplorerService.GetCountryDetailsAsync(name);

        if (country is null)
        {
            return NotFound($"Country '{name}' was not found.");
        }

        var destinationCurrency = country.Country.Currencies.FirstOrDefault();

        if (destinationCurrency is null)
        {
            return BadRequest($"No currency data available for '{name}'.");
        }

        var result = await _exchangeRateService.ConvertAsync(destinationCurrency.Code, fromCurrency, amount);

        return Ok(result);
    }
}