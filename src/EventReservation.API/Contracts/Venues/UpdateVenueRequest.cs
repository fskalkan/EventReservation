namespace EventReservation.API.Contracts.Venues;

public sealed record UpdateVenueRequest(
    string Name,
    string City,
    string Address,
    int Capacity);