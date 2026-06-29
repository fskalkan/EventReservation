using FluentValidation;

namespace EventReservation.Application.Features.EventReports.GetEventReservationsReport;

public sealed class GetEventReservationsReportQueryValidator
    : AbstractValidator<GetEventReservationsReportQuery>
{
    public GetEventReservationsReportQueryValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}