using CountryExplorer.Application.DTOs.Attractions;

namespace CountryExplorer.Application.Interfaces.External;

public interface ITouristAttractionService
{
    Task<List<TouristAttractionDto>> GetAttractionsByCoordinatesAsync(double latitude, double longitude);
    Task<TouristAttractionDetailsDto?> GetAttractionDetailsAsync(string xid);
}