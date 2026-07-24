using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Project_3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    /// <summary>
    /// Get current authenticated user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user id" });


            var user = await _userService.GetCurrentUserAsync(userId, ct);

            if (user == null)
                return NotFound(new { message = "User not found" });


            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            throw;
        }
    }


    /// <summary>
    /// Get all users (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(ct);

            return Ok(new
            {
                totalUsers = users.Count,
                users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            throw;
        }
    }


    /// <summary>
    /// Get users by role.
    /// </summary>
    [Authorize(Policy = "Admin")]
    [HttpGet("role/{role}")]
    public async Task<IActionResult> GetUsersByRole(
        UserRole role,
        CancellationToken ct)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role, ct);

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users by role");
            throw;
        }
    }


    /// <summary>
    /// Promote user to admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{userId}/promote")]
    public async Task<IActionResult> PromoteToAdmin(
        Guid userId,
        CancellationToken ct)
    {
        try
        {
            var adminId = GetCurrentUserId();

            await _userService.PromoteToAdminAsync(
                userId,
                adminId,
                ct);


            return Ok(new
            {
                message = "User promoted to admin"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting user");
            throw;
        }
    }


    /// <summary>
    /// Demote admin to user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{userId}/demote")]
    public async Task<IActionResult> DemoteFromAdmin(
        Guid userId,
        CancellationToken ct)
    {
        try
        {
            var adminId = GetCurrentUserId();

            await _userService.DemoteFromAdminAsync(
                userId,
                adminId,
                ct);


            return Ok(new
            {
                message = "Admin demoted to user"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error demoting admin");
            throw;
        }
    }


    /// <summary>
    /// Soft delete user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(
        Guid userId,
        CancellationToken ct)
    {
        try
        {
            var adminId = GetCurrentUserId();

            await _userService.DeleteUserAsync(
                userId,
                adminId,
                ct);


            return Ok(new
            {
                message = "User deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            throw;
        }
    }


    private Guid GetCurrentUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Invalid user id");

        return userId;
    }
}
