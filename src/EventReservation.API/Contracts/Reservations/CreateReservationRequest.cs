namespace EventReservation.API.Contracts.Reservations;

public sealed record CreateReservationRequest(
    Guid EventId,
    IReadOnlyList<Guid> EventSeatIds);