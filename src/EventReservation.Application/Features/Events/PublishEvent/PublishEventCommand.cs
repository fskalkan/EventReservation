using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.PublishEvent;

public sealed record PublishEventCommand(Guid Id) : ICommand<EventResponse>;