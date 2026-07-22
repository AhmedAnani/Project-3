using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithRefreshTokensAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string plainToken, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    void RemoveRefreshToken(RefreshToken token);
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<List<User>> GetAllUsersAsync(CancellationToken ct = default);
    Task<List<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<int> CountUsersAsync(CancellationToken ct = default);
    Task<int> CountUsersByRoleAsync(UserRole role, CancellationToken ct = default);
}

