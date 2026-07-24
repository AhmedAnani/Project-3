using CountryExplorer.Application.DTOs;
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

    public async Task<PagedResult<TripBucketItem>> GetAllForUserAsync(Guid userId, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.TripBucketItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TripBucketItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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
