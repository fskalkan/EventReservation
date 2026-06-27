using FluentValidation;

namespace EventReservation.Application.Features.Events.CompleteEvent;

public sealed class CompleteEventCommandValidator : AbstractValidator<CompleteEventCommand>
{
    public CompleteEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}