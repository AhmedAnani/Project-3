using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Exceptions;
using CountryExplorer.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CountryExplorer.Api.Controllers;

/// <summary>
/// Handles authentication operations including OAuth login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserRepository userRepo, IAuthService authService,
        ILogger<AuthController> logger)
    {
        _userRepo = userRepo;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates Google OAuth login flow.
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login()
    {

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("OAuthComplete")
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handles OAuth callback after successful Google authentication.
    /// Creates or updates user and returns JWT tokens.
    /// </summary>
    [HttpGet("oauth-complete")]
    public async Task<IActionResult> OAuthComplete()
    {
        try
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Google authentication failed. Reason: {Failure}",
                    result.Failure?.Message);
                return Unauthorized(new { message = "Google authentication failed." });
            }

            // Extract claims from Google token
            var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pictureUrl = result.Principal?.FindFirst("picture")?.Value;

            // Validate required claims
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Google callback missing email claim");
                return Unauthorized(new { message = "Email not provided by Google." });
            }

            if (string.IsNullOrEmpty(googleId))
            {
                _logger.LogWarning("Google callback missing NameIdentifier claim");
                return Unauthorized(new { message = "Google ID not provided." });
            }

            var user = await _userRepo.GetByEmailAsync(email);
            var pictureUrls = result.Principal?.FindFirst("picture")?.Value;
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = name ?? "Google User",
                    GoogleId = googleId,
                    PictureUrl = pictureUrls 
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();

                _logger.LogInformation("New user created via Google OAuth. UserId: {UserId}, Email: {Email}",
                    user.Id, user.Email);
            }
            else if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = googleId;
                if (!string.IsNullOrEmpty(pictureUrl) && string.IsNullOrEmpty(user.PictureUrl))
                {
                    user.PictureUrl = pictureUrl;
                }

                await _userRepo.UpdateAsync(user);
                await _userRepo.SaveChangesAsync();

                _logger.LogInformation("Linked Google account to existing user. UserId: {UserId}", user.Id);
            }

            var tokens = await _authService.GenerateTokensForUserAsync(user);

           

            _logger.LogInformation("User logged in successfully. UserId: {UserId}", user.Id);

            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in OAuthComplete");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred during authentication." });
        }
    }

    /// <summary>
    /// Refreshes expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var result = await _authService.RefreshAccessTokenAsync(dto.RefreshToken);
            return Ok(result);
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogWarning("Invalid refresh token attempt: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while refreshing token." });
        }
    }

    /// <summary>
    /// Logs out the user by invalidating their refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            await _authService.LogoutAsync(dto.RefreshToken);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User logged out. UserId: {UserId}", userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred during logout." });
        }
    }

    /// <summary>
    /// Returns current authenticated user's information.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token." });
            }

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                fullName = user.FullName,
                pictureUrl = user.PictureUrl,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user information");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving user information." });
        }
    }
}