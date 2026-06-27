using FluentValidation;

namespace EventReservation.Application.Features.Seats.GenerateVenueSeats;

public sealed class GenerateVenueSeatsCommandValidator : AbstractValidator<GenerateVenueSeatsCommand>
{
    public GenerateVenueSeatsCommandValidator()
    {
        RuleFor(x => x.VenueId)
            .NotEmpty()
            .WithMessage("Venue id is required.");

        RuleFor(x => x.Section)
            .NotEmpty()
            .WithMessage("Section is required.")
            .MaximumLength(50)
            .WithMessage("Section must not exceed 50 characters.");

        RuleFor(x => x.Row)
            .NotEmpty()
            .WithMessage("Row is required.")
            .MaximumLength(50)
            .WithMessage("Row must not exceed 50 characters.");

        RuleFor(x => x.StartNumber)
            .GreaterThan(0)
            .WithMessage("Start number must be greater than zero.");

        RuleFor(x => x.EndNumber)
            .GreaterThanOrEqualTo(x => x.StartNumber)
            .WithMessage("End number must be greater than or equal to start number.");
    }
}