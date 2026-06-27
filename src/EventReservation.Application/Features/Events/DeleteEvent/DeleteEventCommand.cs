using EventReservation.Application.Abstractions.Messaging;

namespace EventReservation.Application.Features.Events.DeleteEvent;

public sealed record DeleteEventCommand(Guid Id) : ICommand<DeleteEventResponse>;