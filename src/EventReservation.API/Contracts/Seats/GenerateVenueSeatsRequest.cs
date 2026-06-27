namespace EventReservation.API.Contracts.Seats;

public sealed record GenerateVenueSeatsRequest(
    string Section,
    string Row,
    int StartNumber,
    int EndNumber);