namespace EventReservation.API.Contracts.Events;

public sealed record UpdateEventRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate);