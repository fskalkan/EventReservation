using FluentValidation;

namespace EventReservation.Application.Features.Venues.GetVenueById;

public sealed class GetVenueByIdQueryValidator : AbstractValidator<GetVenueByIdQuery>
{
    public GetVenueByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Venue id is required.");
    }
}