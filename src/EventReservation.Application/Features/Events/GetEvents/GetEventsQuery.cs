using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.GetEvents;

public sealed record GetEventsQuery : IQuery<IReadOnlyList<EventResponse>>;