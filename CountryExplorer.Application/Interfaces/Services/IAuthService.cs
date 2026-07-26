using CountryExplorer.Application.DTOs.Auth;
using CountryExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> GenerateTokensForUserAsync(User user); 
    Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<User> HandleGoogleLoginAsync(string email, string name,string googleId,string? pictureUrl,CancellationToken ct = default);
}
