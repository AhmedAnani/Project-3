using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Domain.Exceptions;
using CountryExplorer.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Application.Services;

/// <summary>
/// Handles authentication operations including token generation, refresh, and logout.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepo, IJwtService jwtService, 
        ILogger<AuthService> logger, IMapper mapper)
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

            var response = _mapper.Map<AuthResponseDto>(user);
            response.AccessToken = accessToken;
            response.RefreshToken = refreshTokenEntity.Token;
            response.AccessTokenExpiresAt = expiresAt;

            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tokens for user {UserId}", user?.Id);
            throw new InvalidTokenException("Failed to generate authentication tokens.", ex);
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements token rotation: invalidates old token and creates new one.
    /// </summary>
    /// <exception cref="InvalidTokenException">Thrown when refresh token is invalid or expired.</exception>
    public async Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidTokenException("Refresh token cannot be empty.");

            var storedToken = await _userRepo.GetRefreshTokenAsync(refreshToken);
            
            if (storedToken == null)
                throw new InvalidTokenException("Invalid or expired refresh token.");

            // Validate token is active (not revoked and not expired)
            if (!storedToken.IsActive)
                throw new InvalidTokenException("Refresh token has been revoked or has expired.");

            var user = storedToken.User;
            if (user == null)
                throw new InvalidTokenException("User associated with token not found.");

            // Token rotation: remove old token and create new one
            _userRepo.RemoveRefreshToken(storedToken);
            var newRefreshTokenEntity = _jwtService.CreateRefreshTokenEntity(user.Id);
            await _userRepo.AddRefreshTokenAsync(newRefreshTokenEntity);
            await _userRepo.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes((int)ExpirationTime.AccessToken);

            _logger.LogDebug("Refreshed token for user {UserId}", user.Id);

            var response = _mapper.Map<AuthResponseDto>(user);
            response.AccessToken = accessToken;
            response.RefreshToken = newRefreshTokenEntity.Token;
            response.AccessTokenExpiresAt = expiresAt;

            return response;
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
                _userRepo.RemoveRefreshToken(storedToken);
                await _userRepo.SaveChangesAsync();
                _logger.LogInformation("User {UserId} logged out successfully", storedToken.UserId);
            }
            else
            {
                _logger.LogWarning("Logout attempted with invalid refresh token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }
}