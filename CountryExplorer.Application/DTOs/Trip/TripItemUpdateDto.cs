using System.ComponentModel.DataAnnotations;
using CountryExplorer.Domain.Enums;

namespace CountryExplorer.Application.DTOs.Trip;

public class TripItemUpdateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
    public string CountryCode { get; set; } = string.Empty;


    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public TripStatus Status { get; set; }

    public DateTime? VisitedDate { get; set; }

    public bool SyncWithGoogleCalendar { get; set; } = false;
}
