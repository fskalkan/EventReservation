using EventReservation.Application.Abstractions.Persistence;
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
}