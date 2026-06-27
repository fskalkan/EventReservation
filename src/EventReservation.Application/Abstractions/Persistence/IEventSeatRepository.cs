using EventReservation.Application.Features.EventSeats.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IEventSeatRepository
{
    Task<IReadOnlyList<EventSeatResponse>> GetResponsesByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<EventSeat> eventSeats, CancellationToken cancellationToken = default);
}