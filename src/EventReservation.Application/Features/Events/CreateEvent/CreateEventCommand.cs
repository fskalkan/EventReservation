using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.CreateEvent;
public sealed record CreateEventCommand(
    Guid VenueId,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<EventResponse>;
