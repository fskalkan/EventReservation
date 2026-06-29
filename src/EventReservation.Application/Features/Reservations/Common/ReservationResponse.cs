using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Reservations.Common;

public sealed record ReservationResponse(
    Guid Id,
    string ReservationCode,
    Guid EventId,
    Guid CustomerId,
    ReservationStatus Status,
    decimal TotalAmount,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    IReadOnlyList<ReservationSeatResponse> Seats);