using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.EventSeats.Common;
using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence.Repositories;

public sealed class EventSeatRepository : IEventSeatRepository
{
    private readonly AppDbContext _context;

    public EventSeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EventSeatResponse>> GetResponsesByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.EventSeats
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Seat.Section)
            .ThenBy(x => x.Seat.Row)
            .ThenBy(x => x.Seat.Number)
            .Select(x => new EventSeatResponse(
                x.Id,
                x.EventId,
                x.SeatId,
                x.Seat.Section,
                x.Seat.Row,
                x.Seat.Number,
                x.Seat.Section + "-" + x.Seat.Row + "-" + x.Seat.Number,
                x.Price,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.EventSeats
            .AnyAsync(x => x.EventId == eventId, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<EventSeat> eventSeats, CancellationToken cancellationToken = default)
    {
        await _context.EventSeats.AddRangeAsync(eventSeats, cancellationToken);
    }

    public async Task<IReadOnlyList<EventSeat>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.EventSeats
            .Include(x => x.Seat)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}