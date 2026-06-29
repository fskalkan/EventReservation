namespace EventReservation.Application.Features.Reservations.Common;

public sealed record ReservationSeatResponse(
    Guid EventSeatId,
    string Label,
    decimal Price);