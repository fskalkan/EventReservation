using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Reservations.PayReservation;

public sealed record PayReservationCommand(
    Guid ReservationId,
    decimal Amount,
    PaymentMethod Method) : ICommand<PayReservationResponse>;