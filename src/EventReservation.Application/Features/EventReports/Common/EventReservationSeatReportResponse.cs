namespace EventReservation.Application.Features.EventReports.Common;

public sealed record EventReservationSeatReportResponse(
    Guid EventSeatId,
    string Label,
    decimal Price);