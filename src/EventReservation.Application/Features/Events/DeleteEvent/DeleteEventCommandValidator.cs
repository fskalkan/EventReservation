using FluentValidation;

namespace EventReservation.Application.Features.Events.DeleteEvent;

public sealed class DeleteEventCommandValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}