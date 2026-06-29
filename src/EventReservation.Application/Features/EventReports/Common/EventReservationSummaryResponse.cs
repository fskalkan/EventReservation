namespace EventReservation.Application.Features.EventReports.Common;

public sealed record EventReservationSummaryResponse(
    Guid EventId,
    int TotalSeats,
    int AvailableSeats,
    int LockedSeats,
    int ReservedSeats,
    int TotalReservations,
    int PendingPaymentReservations,
    int ConfirmedReservations,
    int CancelledReservations,
    int ExpiredReservations,
    decimal ConfirmedRevenue);