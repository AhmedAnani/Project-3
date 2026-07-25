using CountryExplorer.Application.Interfaces.Repositories;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CountryExplorer.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDestinationRepository"/>.
/// </summary>
public class DestinationRepository : IDestinationRepository
{
    private readonly AppDbContext _db;

    public DestinationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Destination>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Destinations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Destination>> GetByTagsAsync(VibeTag tags, CancellationToken cancellationToken = default)
    {
        // Bitwise AND in EF Core — returns destinations where ANY of the requested tags match
        return await _db.Destinations
            .AsNoTracking()
            .Where(d => ((int)d.Tags & (int)tags) != 0)
            .ToListAsync(cancellationToken);
    }
}
