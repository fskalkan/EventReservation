namespace EventReservation.Application.Abstractions.Realtime;

public interface IRealtimeNotifier
{
    Task NotifyEventSeatsChangedAsync(Guid eventId, IReadOnlyList<EventSeatStatusChangedMessage> seats, CancellationToken cancellationToken = default);
}