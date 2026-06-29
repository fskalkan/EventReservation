using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.Reservations.Common;
using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }

    public async Task<Reservation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(x => x.Payment)
            .Include(x => x.ReservationSeats)
                .ThenInclude(x => x.EventSeat)
                    .ThenInclude(x => x.Seat)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ReservationResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ReservationResponse(
                x.Id,
                x.ReservationCode,
                x.EventId,
                x.CustomerId,
                x.Status,
                x.TotalAmount,
                x.ExpiresAt,
                x.CreatedAt,
                x.ReservationSeats
                    .Select(rs => new ReservationSeatResponse(
                        rs.EventSeatId,
                        rs.EventSeat.Seat.Section + "-" + rs.EventSeat.Seat.Row + "-" + rs.EventSeat.Seat.Number,
                        rs.Price))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetResponsesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReservationResponse(
                x.Id,
                x.ReservationCode,
                x.EventId,
                x.CustomerId,
                x.Status,
                x.TotalAmount,
                x.ExpiresAt,
                x.CreatedAt,
                x.ReservationSeats
                    .Select(rs => new ReservationSeatResponse(
                        rs.EventSeatId,
                        rs.EventSeat.Seat.Section + "-" + rs.EventSeat.Seat.Row + "-" + rs.EventSeat.Seat.Number,
                        rs.Price))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}