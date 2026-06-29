using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.EventReports.Common;
using EventReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence.Repositories;

public sealed class EventReportRepository : IEventReportRepository
{
    private readonly AppDbContext _context;

    public EventReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EventReservationReportResponse>> GetReservationsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new EventReservationReportResponse(
                x.Id,
                x.ReservationCode,
                x.CustomerId,
                x.Customer.FullName,
                x.Status,
                x.TotalAmount,
                x.ExpiresAt,
                x.CreatedAt,
                x.ConfirmedAt,
                x.CancelledAt,
                x.ExpiredAt,
                x.Payment == null ? null : (PaymentStatus?)x.Payment.Status,
                x.Payment == null ? null : (PaymentMethod?)x.Payment.Method,
                x.Payment == null ? null : x.Payment.PaidAt,
                x.ReservationSeats
                    .Select(rs => new EventReservationSeatReportResponse(
                        rs.EventSeatId,
                        rs.EventSeat.Seat.Section + "-" + rs.EventSeat.Seat.Row + "-" + rs.EventSeat.Seat.Number,
                        rs.Price))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<EventReservationSummaryResponse> GetSummaryByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var seatSummary = await _context.EventSeats
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalSeats = g.Count(),
                AvailableSeats = g.Count(x => x.Status == EventSeatStatus.Available),
                LockedSeats = g.Count(x => x.Status == EventSeatStatus.Locked),
                ReservedSeats = g.Count(x => x.Status == EventSeatStatus.Reserved)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var reservationSummary = await _context.Reservations
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalReservations = g.Count(),
                PendingPaymentReservations = g.Count(x => x.Status == ReservationStatus.PendingPayment),
                ConfirmedReservations = g.Count(x => x.Status == ReservationStatus.Confirmed),
                CancelledReservations = g.Count(x => x.Status == ReservationStatus.Cancelled),
                ExpiredReservations = g.Count(x => x.Status == ReservationStatus.Expired),
                ConfirmedRevenue = g
                    .Where(x => x.Status == ReservationStatus.Confirmed)
                    .Sum(x => (decimal?)x.TotalAmount) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new EventReservationSummaryResponse(
            eventId,
            seatSummary?.TotalSeats ?? 0,
            seatSummary?.AvailableSeats ?? 0,
            seatSummary?.LockedSeats ?? 0,
            seatSummary?.ReservedSeats ?? 0,
            reservationSummary?.TotalReservations ?? 0,
            reservationSummary?.PendingPaymentReservations ?? 0,
            reservationSummary?.ConfirmedReservations ?? 0,
            reservationSummary?.CancelledReservations ?? 0,
            reservationSummary?.ExpiredReservations ?? 0,
            reservationSummary?.ConfirmedRevenue ?? 0);
    }
}