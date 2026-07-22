using CountryExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Services.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
    bool VerifyRefreshToken(string plainToken, string hashedToken);
    RefreshToken CreateRefreshTokenEntity(Guid userId);
}
