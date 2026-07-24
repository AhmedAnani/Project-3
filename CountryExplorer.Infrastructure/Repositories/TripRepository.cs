using CountryExplorer.Application.DTOs;
using CountryExplorer.Application.Interfaces.Repositories;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CountryExplorer.Infrastructure.Repositories;

public class TripRepository : ITripRepository
{
    private readonly AppDbContext _context;

    public TripRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TripBucketItem?> GetByIdAndUserAsync(int tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TripBucketItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == tripId && item.UserId == userId, cancellationToken);
    }

    public async Task<TripBucketItem?> GetByIdAndUserForUpdateAsync(int tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Loaded with EF Core change tracking enabled for state updates
        return await _context.TripBucketItems
            .FirstOrDefaultAsync(item => item.Id == tripId && item.UserId == userId, cancellationToken);
    }

    public async Task<PagedResult<TripBucketItem>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.TripBucketItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TripBucketItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TripBucketItem> AddAsync(TripBucketItem trip, CancellationToken cancellationToken = default)
    {
        _context.TripBucketItems.Add(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return trip;
    }

    public async Task<TripBucketItem> UpdateAsync(TripBucketItem trip, CancellationToken cancellationToken = default)
    {
        // Entity is tracked by ChangeTracker from GetByIdAndUserForUpdateAsync; SaveChangesAsync emits optimized column updates
        await _context.SaveChangesAsync(cancellationToken);
        return trip;
    }

    public async Task<bool> DeleteAsync(int tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        var trip = await _context.TripBucketItems
            .FirstOrDefaultAsync(item => item.Id == tripId && item.UserId == userId, cancellationToken);

        if (trip is null)
        {
            return false;
        }

        trip.IsDeleted = true;
        trip.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
