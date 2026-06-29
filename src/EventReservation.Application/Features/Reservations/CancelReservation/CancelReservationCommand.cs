using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Reservations.Common;

namespace EventReservation.Application.Features.Reservations.CancelReservation;

public sealed record CancelReservationCommand(Guid ReservationId)
    : ICommand<ReservationResponse>;