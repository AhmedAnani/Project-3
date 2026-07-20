using CountryExplorer.Application.Interfaces;
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

    public async Task<TripBucketItem?> GetByIdAndUserAsync(int tripId, Guid userId)
    {
        return await _context.TripBucketItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == tripId && item.UserId == userId);
    }

    public async Task<IEnumerable<TripBucketItem>> GetAllForUserAsync(Guid userId, int skip, int take)
    {
        return await _context.TripBucketItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<TripBucketItem> AddAsync(TripBucketItem trip)
    {
        _context.TripBucketItems.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task<TripBucketItem> UpdateAsync(TripBucketItem trip)
    {
        _context.TripBucketItems.Update(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task<bool> DeleteAsync(int tripId, Guid userId)
    {
        var trip = await _context.TripBucketItems
            .FirstOrDefaultAsync(item => item.Id == tripId && item.UserId == userId);

        if (trip is null)
        {
            return false;
        }

        trip.IsDeleted = true;
        trip.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
