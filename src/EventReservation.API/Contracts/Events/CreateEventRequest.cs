namespace EventReservation.API.Contracts.Events;

public sealed record CreateEventRequest(
    Guid VenueId,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate);