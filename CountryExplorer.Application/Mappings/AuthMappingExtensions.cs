using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CountryExplorer.Application.Mappings;

/// <summary>
/// Extension methods for convenient auth response mapping.
/// Usage: mapper.MapToAuthResponse(user, accessToken, refreshToken, expiresAt);
/// </summary>
public static class AuthMappingExtensions
{
    /// <summary>
    /// Maps a user to AuthResponseDto with auth tokens.
    /// This is the recommended way to create auth responses with generated values.
    /// </summary>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="user">Authenticated user entity</param>
    /// <param name="accessToken">Generated JWT access token</param>
    /// <param name="refreshToken">Generated refresh token (plain text for client)</param>
    /// <param name="accessTokenExpiresAt">Access token expiration time</param>
    /// <returns>Mapped AuthResponseDto with all auth tokens included</returns>
    public static AuthResponseDto MapToAuthResponse(
        this IMapper mapper,
        User user,
        string accessToken,
        string refreshToken,
        DateTime accessTokenExpiresAt)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var response = mapper.Map<AuthResponseDto>(user);

        response.AccessToken = accessToken;
        response.RefreshToken = refreshToken;
        response.AccessTokenExpiresAt = accessTokenExpiresAt;

        return response;
    }

    /// <summary>
    /// Maps user to profile DTO (no auth tokens).
    /// Use for GET /me endpoint.
    /// </summary>
    public static UserProfileDto MapToUserProfile(this IMapper mapper, User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return mapper.Map<UserProfileDto>(user);
    }

    /// <summary>
    /// Maps refresh token to metadata DTO (no token value).
    /// Use for exposing token status without revealing the actual token.
    /// </summary>
    public static RefreshTokenDto MapToRefreshTokenDto(this IMapper mapper, RefreshToken token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        return mapper.Map<RefreshTokenDto>(token);
    }
}
