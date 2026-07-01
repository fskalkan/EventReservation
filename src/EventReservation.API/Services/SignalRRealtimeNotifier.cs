using EventReservation.API.Hubs;
using EventReservation.Application.Abstractions.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace EventReservation.API.Services;

public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<EventSeatsHub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<EventSeatsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyEventSeatsChangedAsync(Guid eventId,IReadOnlyList<EventSeatStatusChangedMessage> seats, CancellationToken cancellationToken = default)
    {
        await _hubContext
            .Clients
            .Group(EventSeatsHub.GetGroupName(eventId))
            .SendAsync(
                "eventSeatsChanged",
                seats,
                cancellationToken);
    }
}