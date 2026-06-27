using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Seats.Common;

namespace EventReservation.Application.Features.Seats.GenerateVenueSeats;

public sealed record GenerateVenueSeatsCommand(
    Guid VenueId,
    string Section,
    string Row,
    int StartNumber,
    int EndNumber) : ICommand<IReadOnlyList<SeatResponse>>;