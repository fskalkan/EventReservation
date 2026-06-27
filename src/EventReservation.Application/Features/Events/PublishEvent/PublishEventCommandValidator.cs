using FluentValidation;

namespace EventReservation.Application.Features.Events.PublishEvent;

public sealed class PublishEventCommandValidator : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}