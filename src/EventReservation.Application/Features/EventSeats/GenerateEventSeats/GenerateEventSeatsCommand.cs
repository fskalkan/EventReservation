using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.EventSeats.Common;

namespace EventReservation.Application.Features.EventSeats.GenerateEventSeats;

public sealed record GenerateEventSeatsCommand(
    Guid EventId,
    decimal DefaultPrice,
    Dictionary<string, decimal> SectionPrices) : ICommand<IReadOnlyList<EventSeatResponse>>;