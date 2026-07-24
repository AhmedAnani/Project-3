using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Domain.Repositories;
using CountryExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(googleId))
            return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithRefreshTokensAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            return null;

        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _context.Users.AddAsync(user, ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a refresh token by verifying the plain token against stored hash.
    /// </summary>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string plainToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
            return null;

        var tokens = await _context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        var matchedToken = tokens.FirstOrDefault(rt =>
            BCrypt.Net.BCrypt.Verify(plainToken, rt.Token));

        return matchedToken;
    }

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        await _context.RefreshTokens.AddAsync(token, ct);
    }

    public void RemoveRefreshToken(RefreshToken token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        _context.RefreshTokens.Remove(token);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error");
            throw;
        }
    }

    /// <summary>
    /// Get all users (excluding soft-deleted).
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.Users
                 .OrderByDescending(u => u.CreatedAt)
                 .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            throw;
        }
    }

    /// <summary>
    /// Get users by specific role (excluding soft-deleted).
    /// </summary>
    public async Task<List<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default)
    {
        try
        {
            return await _context.Users
                .Where( u=> u.Role == role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users by role {Role}", role);
            throw;
        }
    }

    public async Task<User?> GetByEmailIncludingDeletedAsync(string email,CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}