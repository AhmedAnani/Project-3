using CountryExplorer.Domain.Entities;

namespace CountryExplorer.Application.Services.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
    bool VerifyRefreshToken(string plainToken, string hashedToken);
    RefreshToken CreateRefreshTokenEntity(Guid userId);
}
