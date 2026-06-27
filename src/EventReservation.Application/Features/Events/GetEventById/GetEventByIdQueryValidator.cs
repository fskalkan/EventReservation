using FluentValidation;

namespace EventReservation.Application.Features.Events.GetEventById;

public sealed class GetEventByIdQueryValidator : AbstractValidator<GetEventByIdQuery>
{
    public GetEventByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}