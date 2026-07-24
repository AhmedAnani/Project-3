using CountryExplorer.Application.DTOs;
using CountryExplorer.Domain.Enums;
using FluentValidation;

namespace CountryExplorer.Application.Validators;

public class TripItemUpdateDtoValidator : AbstractValidator<TripItemUpdateDto>
{
    public TripItemUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be exactly 2 characters (ISO 3166-1 alpha-2).");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid trip status value.");

        RuleFor(x => x.VisitedDate)
            .NotNull()
            .When(x => x.Status == TripStatus.Visited)
            .WithMessage("Visited date is required when status is marked as Visited.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddDays(1))
            .When(x => x.VisitedDate.HasValue)
            .WithMessage("Visited date cannot be set in the future.");
    }
}
