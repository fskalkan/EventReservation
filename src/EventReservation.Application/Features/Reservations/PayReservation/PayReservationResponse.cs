using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Reservations.PayReservation;

public sealed record PayReservationResponse(
    Guid ReservationId,
    string ReservationCode,
    ReservationStatus ReservationStatus,
    Guid PaymentId,
    PaymentStatus PaymentStatus,
    decimal Amount,
    PaymentMethod Method,
    DateTime? PaidAt);