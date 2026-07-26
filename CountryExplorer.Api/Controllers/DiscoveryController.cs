using CountryExplorer.Application.Dtos.Discovery;
using CountryExplorer.Application.DTOs.Discovery;
using CountryExplorer.Application.DTOs.Trip;
using CountryExplorer.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CountryExplorer.Application.DTOs;

namespace CountryExplorer.Api.Controllers;

/// <summary>
/// Destination discovery and recommendation engine.
/// Scores destinations against the user's trip intent and returns ranked results.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscoveryController : ControllerBase
{
    private readonly IDiscoveryService _discoveryService;
    private readonly ILogger<DiscoveryController> _logger;

    public DiscoveryController(
        IDiscoveryService discoveryService,
        ILogger<DiscoveryController> logger)
    {
        _discoveryService = discoveryService;
        _logger = logger;
    }

    /// <summary>
    /// Returns ranked destination recommendations based on the user's trip intent and preferences.
    /// Destinations that exceed the budget ceiling or fail other dealbreaker checks are
    /// marked as disqualified and sorted to the end of the results.
    /// </summary>
    /// <param name="request">The discovery request containing trip purpose, budget ceiling, preferred tags, and result count.</param>
    /// <returns>A ranked list of scored destinations.</returns>
    /// <response code="200">Returns the ranked list of destination recommendations.</response>
    /// <response code="400">Invalid request parameters.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("recommend")]
    [ProducesResponseType(typeof(List<ScoreBreakdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecommendations(
        [FromBody] DiscoveryRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation(
            "Discovery request: Purpose={Purpose}, MaxBudget={MaxBudget}, Tags={Tags}, TopN={TopN}",
            request.Purpose, request.MaxBudget, request.PreferredTags, request.TopN);

        var results = await _discoveryService.GetRecommendationsAsync(request, cancellationToken);

        return Ok(results);
    }

    /// <summary>
    /// Saves a discovered destination to the user's bucket list (Trips).
    /// </summary>
    /// <param name="destinationId">The ID of the destination to save (returned in the ScoreBreakdownDto).</param>
    /// <param name="request">Optional dates for the trip. If omitted, default dates will be used.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The created Trip item.</returns>
    /// <response code="201">Returns the newly created Trip item.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Destination not found.</response>
    [HttpPost("{destinationId}/save")]
    [ProducesResponseType(typeof(TripItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveToTrips(
        int destinationId,
        [FromBody] SaveDiscoveryRequestDto request,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Invalid or missing user ID in token.");
        }

        // Read the Google OAuth token from the request header (same pattern as TripsController)
        Request.Headers.TryGetValue("X-Google-Token", out var googleTokenHeader);
        var googleAccessToken = googleTokenHeader.ToString() != string.Empty ? googleTokenHeader.ToString() : null;

        _logger.LogInformation(
            "SaveToTrips request: DestinationId={DestinationId}, UserId={UserId}, SyncCalendar={Sync}",
            destinationId, userId, request.SyncWithGoogleCalendar);

        var trip = await _discoveryService.SaveToTripsAsync(userId, destinationId, request, googleAccessToken, cancellationToken);

        return CreatedAtAction(
            actionName: "GetTrip",
            controllerName: "Trips",
            routeValues: new { id = trip.Id },
            value: trip);
    }
}
