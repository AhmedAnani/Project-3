using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Application.Mappings;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Application.Exceptions;
using CountryExplorer.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Application.Services;

/// <summary>
/// Handles authentication operations including token generation, refresh, and logout.
/// Uses AutoMapper extension methods for clean DTO mapping.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepo,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IMapper mapper)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Generates JWT access and refresh tokens for a user after successful authentication.
    /// </summary>
    public async Task<AuthResponseDto> GenerateTokensForUserAsync(User user)
    {
        try
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var refreshTokenEntity = _jwtService.CreateRefreshTokenEntity(user.Id);
            await _userRepo.AddRefreshTokenAsync(refreshTokenEntity);
            await _userRepo.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes((int)ExpirationTime.AccessToken);

            _logger.LogDebug("Generated tokens for user {UserId}", user.Id);

            return _mapper.MapToAuthResponse(user, accessToken, refreshTokenEntity.Token, expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tokens for user {UserId}", user?.Id);
            throw new InvalidTokenException("Failed to generate authentication tokens.", ex);
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements secure token rotation: revokes old token and creates new one.
    /// </summary>
    public async Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidTokenException("Refresh token cannot be empty.");

            var storedToken = await _userRepo.GetRefreshTokenAsync(refreshToken);
            if (storedToken == null)
            {
                _logger.LogWarning("Refresh token not found or expired");
                throw new InvalidTokenException("Invalid or expired refresh token.");
            }

            if (!storedToken.IsActive)
            {
                _logger.LogWarning("Refresh token is not active for user {UserId}", storedToken.UserId);
                throw new InvalidTokenException("Refresh token has been revoked or has expired.");
            }

            var user = storedToken.User;
            if (user == null)
                throw new InvalidTokenException("User associated with token not found.");

            storedToken.Revoke();
            await _userRepo.SaveChangesAsync();

            var newRefreshTokenEntity = _jwtService.CreateRefreshTokenEntity(user.Id);
            await _userRepo.AddRefreshTokenAsync(newRefreshTokenEntity);
            await _userRepo.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes((int)ExpirationTime.AccessToken);

            _logger.LogDebug("Refreshed token for user {UserId}", user.Id);

            return _mapper.MapToAuthResponse(user, accessToken, newRefreshTokenEntity.Token, expiresAt);
        }
        catch (InvalidTokenException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            throw new InvalidTokenException("An error occurred while refreshing the token.", ex);
        }
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    public async Task LogoutAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Logout attempted with empty refresh token");
                return;
            }

            var storedToken = await _userRepo.GetRefreshTokenAsync(refreshToken);
            if (storedToken != null)
            {
                storedToken.Revoke();
                await _userRepo.SaveChangesAsync();
                _logger.LogInformation("User {UserId} logged out successfully", storedToken.UserId);
            }
            else
            {
                _logger.LogWarning("Logout attempted with invalid or expired refresh token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }
    /// <summary>
    /// Handles Google login. Creates a new user or restores a soft-deleted user.
    /// </summary>
    public async Task<User> HandleGoogleLoginAsync(
        string email,
        string name,
        string googleId,
        string? pictureUrl,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _userRepo
                .GetByEmailIncludingDeletedAsync(email, ct);


            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = name,
                    GoogleId = googleId,
                    PictureUrl = pictureUrl,
                    Role = UserRole.User,
                    IsDeleted = false
                };


                await _userRepo.AddAsync(user, ct);
                await _userRepo.SaveChangesAsync(ct);


                _logger.LogInformation(
                    "New Google user created {UserId}",
                    user.Id);


                return user;
            }


            var updated = false;


            if (user.IsDeleted)
            {
                user.IsDeleted = false;
                updated = true;


                _logger.LogInformation(
                    "Restored deleted user {UserId}",
                    user.Id);
            }


            if (string.IsNullOrWhiteSpace(user.GoogleId))
            {
                user.GoogleId = googleId;
                updated = true;
            }


            if (!string.IsNullOrWhiteSpace(pictureUrl) &&
                user.PictureUrl != pictureUrl)
            {
                user.PictureUrl = pictureUrl;
                updated = true;
            }


            if (updated)
            {
                await _userRepo.UpdateAsync(user, ct);
                await _userRepo.SaveChangesAsync(ct);
            }


            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling Google login for {Email}",
                email);

            throw new UserCreationException(
                "Failed to process Google login.",
                ex);
        }
    }
}