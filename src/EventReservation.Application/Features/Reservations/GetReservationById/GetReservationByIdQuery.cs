using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Reservations.Common;

namespace EventReservation.Application.Features.Reservations.GetReservationById;

public sealed record GetReservationByIdQuery(Guid ReservationId)
    : IQuery<ReservationResponse>;