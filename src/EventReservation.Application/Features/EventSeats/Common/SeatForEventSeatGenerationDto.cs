namespace EventReservation.Application.Features.Seats.Common;

public sealed record SeatForEventSeatGenerationDto(
    Guid Id,
    string Section);