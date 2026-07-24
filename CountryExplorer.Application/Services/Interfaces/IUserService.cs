using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Services.Interfaces;

public interface IUserService
{
    
        Task<UserProfileDto?> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);

        Task<List<UserProfileDto>> GetAllUsersAsync(CancellationToken ct = default);

        Task<List<UserProfileDto>> GetUsersByRoleAsync(
            UserRole role,
            CancellationToken ct = default);

        Task PromoteToAdminAsync(
            Guid userId,
            Guid adminId,
            CancellationToken ct = default);

        Task DemoteFromAdminAsync(
            Guid userId,
            Guid adminId,
            CancellationToken ct = default);

        Task DeleteUserAsync(
            Guid userId,
            Guid adminId,
            CancellationToken ct = default);
    
}
