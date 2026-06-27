using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.CancelEvent;

public sealed record CancelEventCommand(Guid Id) : ICommand<EventResponse>;