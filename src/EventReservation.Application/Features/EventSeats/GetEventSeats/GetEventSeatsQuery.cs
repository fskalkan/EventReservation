using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.EventSeats.Common;

namespace EventReservation.Application.Features.EventSeats.GetEventSeats;

public sealed record GetEventSeatsQuery(Guid EventId)
    : IQuery<IReadOnlyList<EventSeatResponse>>;