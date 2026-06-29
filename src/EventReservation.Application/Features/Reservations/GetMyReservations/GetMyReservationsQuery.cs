using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Reservations.Common;

namespace EventReservation.Application.Features.Reservations.GetMyReservations;

public sealed record GetMyReservationsQuery()
    : IQuery<IReadOnlyList<ReservationResponse>>;