using FluentValidation;

namespace EventReservation.Application.Features.Events.UpdateEvent;

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Event title is required.")
            .MaximumLength(200)
            .WithMessage("Event title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Event description is required.")
            .MaximumLength(1000)
            .WithMessage("Event description must not exceed 1000 characters.");

        RuleFor(x => x.StartDate)
            .Must(startDate => startDate > DateTime.UtcNow)
            .WithMessage("Start date must be in the future.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be greater than start date.");
    }
}