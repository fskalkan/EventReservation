namespace EventReservation.API.Contracts.EventSeats;

public sealed record GenerateEventSeatsRequest(
    decimal DefaultPrice,
    Dictionary<string, decimal> SectionPrices);