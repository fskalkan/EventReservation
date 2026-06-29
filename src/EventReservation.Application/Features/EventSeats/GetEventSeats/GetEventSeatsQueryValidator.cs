using FluentValidation;

namespace EventReservation.Application.Features.EventSeats.GetEventSeats;

public sealed class GetEventSeatsQueryValidator : AbstractValidator<GetEventSeatsQuery>
{
    public GetEventSeatsQueryValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}