namespace EventReservation.API.Contracts.Venues;

    public sealed record CreateVenueRequest(
        string Name,
        string City,
        string Address,
        int Capacity);