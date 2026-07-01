using EventReservation.Domain.Enums;

namespace EventReservation.Application.Abstractions.Realtime;

public sealed record EventSeatStatusChangedMessage(
    Guid EventId,
    Guid EventSeatId,
    Guid SeatId,
    string Label,
    decimal Price,
    EventSeatStatus Status);