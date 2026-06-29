using EventReservation.Domain.Enums;

namespace EventReservation.API.Contracts.Reservations;

public sealed record PayReservationRequest(
    decimal Amount,
    PaymentMethod Method);