using Microsoft.AspNetCore.SignalR;

namespace EventReservation.API.Hubs;

public sealed class EventSeatsHub : Hub
{
    public Task JoinEventGroup(Guid eventId)
    {
        return Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(eventId));
    }

    public Task LeaveEventGroup(Guid eventId)
    {
        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(eventId));
    }

    public static string GetGroupName(Guid eventId)
    {
        return $"event-seats-{eventId}";
    }
}