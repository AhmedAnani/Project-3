using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Destination data access.
/// </summary>
public interface IDestinationRepository
{
    /// <summary>
    /// Retrieves all destinations.
    /// </summary>
    Task<List<Destination>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves destinations that match any of the specified VibeTag flags.
    /// </summary>
    Task<List<Destination>> GetByTagsAsync(VibeTag tags, CancellationToken cancellationToken = default);
}
