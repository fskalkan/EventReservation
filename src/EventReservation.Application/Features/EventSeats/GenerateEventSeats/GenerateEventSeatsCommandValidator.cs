using FluentValidation;

namespace EventReservation.Application.Features.EventSeats.GenerateEventSeats;

public sealed class GenerateEventSeatsCommandValidator : AbstractValidator<GenerateEventSeatsCommand>
{
    public GenerateEventSeatsCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.DefaultPrice)
            .GreaterThan(0)
            .WithMessage("Default price must be greater than zero.");

        RuleFor(x => x.SectionPrices)
            .NotNull()
            .WithMessage("Section prices are required.");

        RuleForEach(x => x.SectionPrices)
            .Must(x => !string.IsNullOrWhiteSpace(x.Key))
            .WithMessage("Section is required.")
            .Must(x => x.Value > 0)
            .WithMessage("Section price must be greater than zero.");
    }
}