using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Seats.Common;

namespace EventReservation.Application.Features.Seats.GetVenueSeats;

public sealed record GetVenueSeatsQuery(Guid VenueId)
    : IQuery<IReadOnlyList<SeatResponse>>;