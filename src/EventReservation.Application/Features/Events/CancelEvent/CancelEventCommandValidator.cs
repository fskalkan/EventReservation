using FluentValidation;

namespace EventReservation.Application.Features.Events.CancelEvent;

public sealed class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}