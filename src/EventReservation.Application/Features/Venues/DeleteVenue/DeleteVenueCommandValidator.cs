using FluentValidation;

namespace EventReservation.Application.Features.Venues.DeleteVenue;

public sealed class DeleteVenueCommandValidator : AbstractValidator<DeleteVenueCommand>
{
    public DeleteVenueCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Venue id is required.");
    }
}