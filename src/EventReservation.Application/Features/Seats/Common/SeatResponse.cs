namespace EventReservation.Application.Features.Seats.Common;

public sealed record SeatResponse(
    Guid Id,
    Guid VenueId,
    string Section,
    string Row,
    int Number,
    string Label,
    DateTime CreatedAt);