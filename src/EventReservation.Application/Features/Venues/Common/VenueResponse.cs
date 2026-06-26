namespace EventReservation.Application.Features.Venues.Common;

public sealed record VenueResponse(
    Guid Id,
    string Name,
    string City,
    string Address,
    int Capacity,
    Guid CreatedByUserId,
    DateTime CreatedAt);