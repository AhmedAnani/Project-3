using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Repositories;
using CountryExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CountryExplorer.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        => await _context.RefreshTokens.AddAsync(token, ct);

    public void RemoveRefreshToken(RefreshToken token)
        => _context.RefreshTokens.Remove(token);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}