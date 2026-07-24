using System.Security.Claims;
using CountryExplorer.Application.DTOs;
using CountryExplorer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountryExplorer.Api.Controllers;

/// <summary>
/// Provides REST API endpoints for managing user trips and their synchronization with Google Calendar.
/// All endpoints require authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController(ITripService tripService) : ControllerBase
{
    /// <summary>
    /// Extracts the user ID from the authenticated user's claims.
    /// </summary>
    /// <returns>The user ID as a GUID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID cannot be extracted from claims.</exception>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID could not be extracted from claims.");
        }
        return userId;
    }

    /// <summary>
    /// Retrieves the Google OAuth token from the X-Google-Token header if present.
    /// </summary>
    /// <returns>The Google token or null if not provided.</returns>
    private string? GetGoogleToken()
    {
        Request.Headers.TryGetValue("X-Google-Token", out var token);
        return token.ToString() != string.Empty ? token.ToString() : null;
    }

    /// <summary>
    /// Retrieves all trips for the authenticated user with pagination support.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (default: 1).</param>
    /// <param name="pageSize">The number of records to return per page (default: 20).</param>
    /// <returns>A paginated list of trips.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TripItemResponseDto>>> GetTrips([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var trips = await tripService.GetAllTripsAsync(userId, pageNumber, pageSize);
        return Ok(trips);
    }

    /// <summary>
    /// Retrieves a specific trip by ID.
    /// </summary>
    /// <param name="id">The ID of the trip to retrieve.</param>
    /// <returns>The trip details or 404 if not found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TripItemResponseDto>> GetTrip(int id)
    {
        var userId = GetUserId();
        var trip = await tripService.GetTripAsync(userId, id);

        if (trip == null)
        {
            return NotFound(new { message = $"Trip with ID {id} not found." });
        }

        return Ok(trip);
    }

    /// <summary>
    /// Creates a new trip for the authenticated user.
    /// </summary>
    /// <param name="dto">The trip creation data.</param>
    /// <returns>The created trip with a Location header (201 Created).</returns>
    [HttpPost]
    public async Task<ActionResult<TripItemResponseDto>> CreateTrip([FromBody] TripItemCreateDto dto)
    {
        var userId = GetUserId();
        var googleToken = GetGoogleToken();

        var createdTrip = await tripService.CreateTripAsync(userId, dto, googleToken);

        return CreatedAtAction(nameof(GetTrip), new { id = createdTrip.Id }, createdTrip);
    }

    /// <summary>
    /// Updates an existing trip.
    /// </summary>
    /// <param name="id">The ID of the trip to update.</param>
    /// <param name="dto">The updated trip data.</param>
    /// <returns>204 No Content on success.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrip(int id, [FromBody] TripItemUpdateDto dto)
    {
        var userId = GetUserId();
        var googleToken = GetGoogleToken();

        await tripService.UpdateTripAsync(userId, id, dto, googleToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a specific trip.
    /// </summary>
    /// <param name="id">The ID of the trip to delete.</param>
    /// <returns>204 No Content on success.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var userId = GetUserId();
        var googleToken = GetGoogleToken();

        await tripService.DeleteTripAsync(userId, id, googleToken);

        return NoContent();
    }
}
