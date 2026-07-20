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
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("Login initiated");
            _logger.LogDebug("Request scheme: {Scheme}, Host: {Host}",
                Request.Scheme, Request.Host);

            // ✅ Generate fully qualified callback URL
            var callbackUrl = Url.Action("OAuthComplete", "Auth", null, Request.Scheme);

            _logger.LogDebug("Callback URL: {CallbackUrl}", callbackUrl);

            var properties = new AuthenticationProperties
            {
                RedirectUri = callbackUrl,
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme },
                    { "returnUrl", returnUrl ?? "/" }
                }
            };

            _logger.LogInformation("Initiating Google challenge");

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating login");
            return BadRequest(new { message = "Failed to initiate login" });
        }
    }

    /// <summary>
    /// Handles OAuth callback from Google.
    /// </summary>
    [HttpGet("oauth-complete")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthComplete()
    {
        try
        {
            _logger.LogInformation("OAuth complete called");

            var cookies = string.Join(", ", HttpContext.Request.Cookies.Keys);
            _logger.LogDebug("Cookies received: {Cookies}", cookies);

            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                _logger.LogError("Authentication failed: {Failure}", result.Failure?.Message);
                return Unauthorized(new { message = "Google authentication failed" });
            }

            var principal = result.Principal;
            if (principal == null)
            {
                _logger.LogError("No principal in result");
                return Unauthorized(new { message = "Invalid authentication state" });
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pictureUrl = principal.FindFirst("picture")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                _logger.LogError("Missing email or googleId");
                return Unauthorized(new { message = "Missing required claims" });
            }

            _logger.LogInformation("OAuth successful for email: {Email}", email);

            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = name ?? "Google User",
                    GoogleId = googleId,
                    PictureUrl = pictureUrl
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();
                _logger.LogInformation("User created: {UserId}", user.Id);
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
            }

            var tokens = await _authService.GenerateTokensForUserAsync(user);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OAuthComplete");
            return StatusCode(500, new { message = "Authentication error" });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token required" });
            }

            var result = await _authService.RefreshAccessTokenAsync(dto.RefreshToken);
            return Ok(result);
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogWarning("Invalid refresh token: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { message = "Error refreshing token" });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token required" });
            }

            await _authService.LogoutAsync(dto.RefreshToken);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User logged out: {UserId}", userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "Logout error" });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user ID" });
            }

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
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
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(500, new { message = "Error retrieving user" });
        }
    }
}