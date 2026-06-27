using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.EventSeats.Common;

public sealed record EventSeatResponse(
    Guid Id,
    Guid EventId,
    Guid SeatId,
    string Section,
    string Row,
    int Number,
    string Label,
    decimal Price,
    EventSeatStatus Status,
    DateTime CreatedAt);