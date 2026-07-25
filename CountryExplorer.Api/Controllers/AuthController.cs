using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CountryExplorer.Api.Controllers;

/// <summary>
/// Handles authentication operations including OAuth login, token refresh, and logout.
/// /// With role-based authorization support.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger,
        IConfiguration config)
    {
        _authService = authService;
        _logger = logger;
        _config = config;
    }


    /// <summary>
    /// Initiates Google OAuth login.
    /// </summary>
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!IsValidReturnUrl(returnUrl))
        {
            returnUrl = "/";
        }

        var baseUrl = _config["AppBaseUrl"]
                      ?? $"{Request.Scheme}://{Request.Host}";

        var callbackUrl = $"{baseUrl}/api/auth/oauth-complete";


        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items =
            {
                {
                    "returnUrl",
                    returnUrl ?? "/"
                }
            }
        };


        return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme);
    }



    /// <summary>
    /// Google OAuth callback.
    /// </summary>
    [HttpGet("oauth-complete")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthComplete()
    {
        var result = await HttpContext.AuthenticateAsync(
         CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized(new
            {
                message = "Google authentication failed"
            });
        }

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
        var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pictureUrl = result.Principal.FindFirst("picture")?.Value;

        var googleAccessToken = result.Properties?.GetTokenValue("access_token");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
        {
            return Unauthorized(new { message = "Missing Google claims" });
        }

        if (!IsValidImageUrl(pictureUrl))
        {
            pictureUrl = null;
        }

        var user = await _authService.HandleGoogleLoginAsync(
            email,
            name ?? "Google User",
            googleId,
            pictureUrl,
            googleAccessToken);

        var tokens = await _authService.GenerateTokensForUserAsync(user);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("User logged in successfully {UserId}", user.Id);

        return Ok(new AuthResponseWithGoogleTokenDto
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            GoogleAccessToken = googleAccessToken,  
            User = tokens.User
        });
    }




    /// <summary>
    /// Refresh JWT token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
        {
            return BadRequest(new
            {
                message = "Refresh token is required"
            });
        }


        var result = await _authService
            .RefreshAccessTokenAsync(dto.RefreshToken);


        return Ok(result);
    }




    /// <summary>
    /// Logout user.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
        {
            return BadRequest(new
            {
                message = "Refresh token is required"
            });
        }


        await _authService.LogoutAsync(dto.RefreshToken);


        var userId = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;


        _logger.LogInformation(
            "User logged out {UserId}",
            userId);



        return NoContent();
    }




    private static bool IsValidReturnUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;


        if (!url.StartsWith("/"))
            return false;


        if (url.StartsWith("//"))
            return false;


        if (url.Contains("\r") ||
            url.Contains("\n"))
            return false;


        return true;
    }



    private static bool IsValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;


        try
        {
            var uri = new Uri(url);


            if (uri.Scheme != Uri.UriSchemeHttps)
                return false;


            var trustedHosts = new[]
            {
                "lh3.googleusercontent.com",
                "graph.microsoft.com",
                "avatars.githubusercontent.com"
            };


            return trustedHosts.Any(
                host => uri.Host.Contains(host));
        }
        catch
        {
            return false;
        }
    }

}