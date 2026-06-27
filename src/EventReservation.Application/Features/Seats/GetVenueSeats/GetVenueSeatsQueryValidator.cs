using FluentValidation;

namespace EventReservation.Application.Features.Seats.GetVenueSeats;

public sealed class GetVenueSeatsQueryValidator : AbstractValidator<GetVenueSeatsQuery>
{
    public GetVenueSeatsQueryValidator()
    {
        RuleFor(x => x.VenueId)
            .NotEmpty()
            .WithMessage("Venue id is required.");
    }
}