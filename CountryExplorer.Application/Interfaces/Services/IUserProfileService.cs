using CountryExplorer.Application.DTOs.Auth;

namespace CountryExplorer.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserFullProfileDto?> GetFullProfileAsync(Guid userId, CancellationToken ct = default);
}