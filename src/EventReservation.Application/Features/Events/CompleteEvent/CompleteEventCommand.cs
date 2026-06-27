using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.CompleteEvent;

public sealed record CompleteEventCommand(Guid Id) : ICommand<EventResponse>;