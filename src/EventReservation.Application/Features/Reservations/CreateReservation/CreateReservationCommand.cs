using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Reservations.Common;

namespace EventReservation.Application.Features.Reservations.CreateReservation;

public sealed record CreateReservationCommand(
    Guid EventId,
    IReadOnlyList<Guid> EventSeatIds) : ICommand<ReservationResponse>;