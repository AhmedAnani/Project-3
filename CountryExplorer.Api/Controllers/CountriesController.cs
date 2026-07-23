using CountryExplorer.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CountryExplorer.Api.Controllers;

[ApiController]
[Route("api/countries")]
public class CountriesController : ControllerBase
{
    private readonly ICountryExplorerService _countryExplorerService;
    private readonly ITouristAttractionService _touristAttractionService;

    public CountriesController(
        ICountryExplorerService countryExplorerService,
        ITouristAttractionService touristAttractionService)
    {
        _countryExplorerService = countryExplorerService;
        _touristAttractionService = touristAttractionService;
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
}