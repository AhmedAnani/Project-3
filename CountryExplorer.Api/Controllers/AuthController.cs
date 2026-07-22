using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Application.Mappings;
using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
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
/// /// With role-based authorization support.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;

    public AuthController(
        IUserRepository userRepo,
        IAuthService authService,
        ILogger<AuthController> logger,
        IConfiguration config,
        IMapper mapper)
    {
        _userRepo = userRepo;
        _authService = authService;
        _logger = logger;
        _config = config;
        _mapper = mapper;
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
            _logger.LogInformation("Login initiated with returnUrl: {ReturnUrl}", returnUrl ?? "null");

            if (!IsValidReturnUrl(returnUrl))
            {
                _logger.LogWarning("Invalid returnUrl attempted: {ReturnUrl}", returnUrl);
                returnUrl = "/";
            }

            var baseUrl = _config["AppBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            var callbackUrl = $"{baseUrl}/api/auth/oauth-complete";

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
                _logger.LogError("Missing email or googleId in claims");
                return Unauthorized(new { message = "Missing required claims" });
            }

            if (!string.IsNullOrEmpty(pictureUrl) && !IsValidImageUrl(pictureUrl))
            {
                _logger.LogWarning("Invalid picture URL rejected: {PictureUrl}", pictureUrl);
                pictureUrl = null;
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
                _logger.LogInformation("New user created: {UserId}", user.Id);
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
                _logger.LogInformation("Existing user linked with Google ID: {UserId}", user.Id);
            }

            var tokens = await _authService.GenerateTokensForUserAsync(user);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in OAuthComplete");
            return StatusCode(500, new { message = "Authentication error" });
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
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
            return StatusCode(500, new { message = "Error refreshing token" });
        }
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
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

    /// <summary>
    /// Validates that a return URL is safe (relative and on same origin).
    /// </summary>
    private static bool IsValidReturnUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        if (!url.StartsWith("/"))
            return false;

        if (url.StartsWith("//"))
            return false;

        if (url.Contains("\r") || url.Contains("\n"))
            return false;

        return true;
    }

    /// <summary>
    /// Validates that a URL is a valid HTTPS image URL from trusted hosts.
    /// </summary>
    private static bool IsValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
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

            return trustedHosts.Any(host => uri.Host.Contains(host));
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Get current authenticated user's profile with role information.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid user ID in token");
                return Unauthorized(new { message = "Invalid user ID" });
            }

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            var response = _mapper.MapToUserProfile(user);

            _logger.LogInformation(
                "User {UserId} ({Email}) with role {Role} retrieved profile",
                userId, emailClaim, roleClaim ?? "Unknown");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "Error retrieving user profile" });
        }
    }

    /// <summary>
    /// Check if current user is admin.
    /// </summary>
    [Authorize]
    [HttpGet("is-admin")]
    public IActionResult IsAdmin()
    {
        try
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = roleClaim == "Admin";

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} admin check: {IsAdmin}",
                userIdClaim, isAdmin);

            return Ok(new { isAdmin = isAdmin, role = roleClaim ?? "User" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking admin status");
            return StatusCode(500, new { message = "Error checking admin status" });
        }
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            _logger.LogInformation(
                "Admin {AdminId} ({AdminEmail}) requested all users list",
                adminIdClaim, adminEmailClaim);

            var users = await _userRepo.GetAllUsersAsync();
            var userDtos = _mapper.Map<List<UserProfileDto>>(users);

            return Ok(new
            {
                totalUsers = userDtos.Count,
                users = userDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            return StatusCode(500, new { message = "Error fetching users" });
        }
    }

    /// <summary>
    /// Get users by specific role.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("myRole/{role}")]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        try
        {
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                return BadRequest(new
                {
                    message = "Invalid role. Valid values: User, Admin",
                    validRoles = Enum.GetNames(typeof(UserRole))
                });
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Admin {AdminId} requested users with role {Role}",
                adminIdClaim, role);

            var users = await _userRepo.GetUsersByRoleAsync(userRole);
            var userDtos = _mapper.Map<List<UserProfileDto>>(users);

            return Ok(new
            {
                role = role,
                count = userDtos.Count,
                users = userDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users by role");
            return StatusCode(500, new { message = "Error fetching users by role" });
        }
    }

    /// <summary>
    /// Promote user to admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/{userId}")]
    public async Task<IActionResult> PromoteToAdmin(Guid userId)
    {
        try
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (userId == Guid.Empty)
                return BadRequest(new { message = "Invalid user ID" });

            if (Guid.TryParse(adminIdClaim, out var adminId) && adminId == userId)
                return BadRequest(new { message = "Cannot modify your own role" });

            var targetUser = await _userRepo.GetByIdAsync(userId);
            if (targetUser == null)
                return NotFound(new { message = "User not found" });

            if (targetUser.Role == UserRole.Admin)
                return BadRequest(new { message = "User is already an admin" });

            targetUser.Role = UserRole.Admin;
            await _userRepo.UpdateAsync(targetUser);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {AdminId} ({AdminEmail}) promoted user {UserId} ({UserEmail}) to Admin",
                adminIdClaim, adminEmailClaim, userId, targetUser.Email);

            return Ok(new
            {
                message = "User promoted to admin",
                user = new { id = targetUser.Id, email = targetUser.Email, role = "Admin" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting user to admin");
            return StatusCode(500, new { message = "Error promoting user" });
        }
    }

    /// <summary>
    /// Demote admin to user 
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/demote/{userId}")]
    public async Task<IActionResult> DemoteFromAdmin(Guid userId)
    {
        try
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (userId == Guid.Empty)
                return BadRequest(new { message = "Invalid user ID" });

            if (Guid.TryParse(adminIdClaim, out var adminId) && adminId == userId)
                return BadRequest(new { message = "Cannot modify your own role" });

            var targetUser = await _userRepo.GetByIdAsync(userId);
            if (targetUser == null)
                return NotFound(new { message = "User not found" });

            if (targetUser.Role == UserRole.User)
                return BadRequest(new { message = "User is already a standard user" });

            targetUser.Role = UserRole.User;
            await _userRepo.UpdateAsync(targetUser);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {AdminId} ({AdminEmail}) demoted user {UserId} ({UserEmail}) to User",
                adminIdClaim, adminEmailClaim, userId, targetUser.Email);

            return Ok(new
            {
                message = "Admin demoted to user",
                user = new { id = targetUser.Id, email = targetUser.Email, role = "User" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error demoting admin");
            return StatusCode(500, new { message = "Error demoting admin" });
        }
    }

    /// <summary>
    /// Delete user (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("admin/users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == Guid.Empty)
                return BadRequest(new { message = "Invalid user ID" });

            if (Guid.TryParse(adminIdClaim, out var adminId) && adminId == userId)
                return BadRequest(new { message = "Cannot delete your own account" });

            var targetUser = await _userRepo.GetByIdAsync(userId);
            if (targetUser == null)
                return NotFound(new { message = "User not found" });

            targetUser.IsDeleted = true;
            await _userRepo.UpdateAsync(targetUser);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} deleted user {UserId} ({UserEmail})",
                adminIdClaim, userId, targetUser.Email);

            return Ok(new
            {
                message = "User deleted successfully",
                deletedUser = new { id = targetUser.Id, email = targetUser.Email }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return StatusCode(500, new { message = "Error deleting user" });
        }
    }
}