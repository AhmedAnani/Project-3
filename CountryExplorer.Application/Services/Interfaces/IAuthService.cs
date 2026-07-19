using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> GenerateTokensForUserAsync(User user); // used after OAuth complete
    Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
