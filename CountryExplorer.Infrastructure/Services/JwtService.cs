using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CountryExplorer.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("picture", user.PictureUrl ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes((int)ExpirationTime.AccessToken);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Hashes a refresh token using bcrypt for secure storage.
    /// </summary>
    public string HashRefreshToken(string token)
    {
        return BCrypt.Net.BCrypt.HashPassword(token, workFactor: 11);
    }

    /// <summary>
    /// Verifies a plain refresh token against its hashed version.
    /// </summary>
    public bool VerifyRefreshToken(string plainToken, string hashedToken)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainToken, hashedToken);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a refresh token entity with hashed token value.
    /// </summary>
    public RefreshToken CreateRefreshTokenEntity(Guid userId)
    {
        var plainToken = GenerateRefreshToken();
        var hashedToken = HashRefreshToken(plainToken);

        return new RefreshToken
        {
            Token = hashedToken,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays((int)ExpirationTime.RefreshToken)
        };
    }
}