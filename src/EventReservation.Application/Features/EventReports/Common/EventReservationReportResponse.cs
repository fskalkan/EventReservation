using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.EventReports.Common;

public sealed record EventReservationReportResponse(
    Guid ReservationId,
    string ReservationCode,
    Guid CustomerId,
    string CustomerFullName,
    ReservationStatus ReservationStatus,
    decimal TotalAmount,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    DateTime? ExpiredAt,
    PaymentStatus? PaymentStatus,
    PaymentMethod? PaymentMethod,
    DateTime? PaidAt,
    IReadOnlyList<EventReservationSeatReportResponse> Seats);