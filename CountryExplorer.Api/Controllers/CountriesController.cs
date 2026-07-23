using CountryExplorer.Application.Interfaces.Services;
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