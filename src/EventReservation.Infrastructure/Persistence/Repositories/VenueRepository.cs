using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence.Repositories;

public sealed class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Venues
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndCityAsync(string name, string city, CancellationToken cancellationToken = default)
    {
        return await _context.Venues
            .AnyAsync(
                x => x.Name == name && x.City == city,
                cancellationToken);
    }

    public async Task AddAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        await _context.Venues.AddAsync(venue, cancellationToken);
    }
}