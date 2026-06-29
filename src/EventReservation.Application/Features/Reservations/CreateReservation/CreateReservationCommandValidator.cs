using FluentValidation;

namespace EventReservation.Application.Features.Reservations.CreateReservation;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.EventSeatIds)
            .NotEmpty()
            .WithMessage("At least one event seat must be selected.");

        RuleFor(x => x.EventSeatIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .When(x => x.EventSeatIds is not null)
            .WithMessage("Duplicate event seats are not allowed.");

        RuleForEach(x => x.EventSeatIds)
            .NotEmpty()
            .WithMessage("Event seat id is required.");
    }
}