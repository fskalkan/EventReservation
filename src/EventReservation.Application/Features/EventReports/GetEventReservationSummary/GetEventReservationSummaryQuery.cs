using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.EventReports.Common;

namespace EventReservation.Application.Features.EventReports.GetEventReservationSummary;

public sealed record GetEventReservationSummaryQuery(Guid EventId)
    : IQuery<EventReservationSummaryResponse>;