using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.Seats.Common;
using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence.Repositories;

public sealed class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Seat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SeatResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SeatResponse(
                x.Id,
                x.VenueId,
                x.Section,
                x.Row,
                x.Number,
                x.Section + "-" + x.Row + "-" + x.Number,
                x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SeatResponse>> GetResponsesByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .AsNoTracking()
            .Where(x => x.VenueId == venueId)
            .OrderBy(x => x.Section)
            .ThenBy(x => x.Row)
            .ThenBy(x => x.Number)
            .Select(x => new SeatResponse(
                x.Id,
                x.VenueId,
                x.Section,
                x.Row,
                x.Number,
                x.Section + "-" + x.Row + "-" + x.Number,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .CountAsync(x => x.VenueId == venueId, cancellationToken);
    }

    public async Task<bool> ExistsInRangeAsync(Guid venueId, string section, string row, int startNumber, int endNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .AnyAsync(
                x => x.VenueId == venueId &&
                     x.Section == section &&
                     x.Row == row &&
                     x.Number >= startNumber &&
                     x.Number <= endNumber,
                cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default)
    {
        await _context.Seats.AddRangeAsync(seats, cancellationToken);
    }
}