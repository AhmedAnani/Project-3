using System.ComponentModel.DataAnnotations;

namespace CountryExplorer.Application.DTOs;

public class TripItemCreateDto
{
    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
    public string CountryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country name is required.")]
    [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters.")]
    public string CountryName { get; set; } = string.Empty;

    public DateTime? TargetDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; set; }

    public bool SyncWithGoogleCalendar { get; set; } = false;
}
