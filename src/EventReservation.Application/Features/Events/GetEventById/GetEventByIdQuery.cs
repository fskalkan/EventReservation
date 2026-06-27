using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.GetEventById;

public sealed record GetEventByIdQuery(Guid Id) : IQuery<EventResponse>;