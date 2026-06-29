using FluentValidation;

namespace EventReservation.Application.Features.EventReports.GetEventReservationSummary;

public sealed class GetEventReservationSummaryQueryValidator
    : AbstractValidator<GetEventReservationSummaryQuery>
{
    public GetEventReservationSummaryQueryValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}