using EventReservation.Application.Features.EventReports.Common;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IEventReportRepository
{
    Task<IReadOnlyList<EventReservationReportResponse>> GetReservationsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<EventReservationSummaryResponse> GetSummaryByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}