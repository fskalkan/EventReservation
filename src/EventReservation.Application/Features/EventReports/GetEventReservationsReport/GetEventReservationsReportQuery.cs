using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.EventReports.Common;

namespace EventReservation.Application.Features.EventReports.GetEventReservationsReport;

public sealed record GetEventReservationsReportQuery(Guid EventId)
    : IQuery<IReadOnlyList<EventReservationReportResponse>>;