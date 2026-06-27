using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.Events.Common;
using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<EventResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EventResponse(
                x.Id,
                x.VenueId,
                x.Venue.Name,
                x.OrganizerId,
                x.Title,
                x.Description,
                x.StartDate,
                x.EndDate,
                x.Status,
                x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .OrderBy(x => x.StartDate)
            .Select(x => new EventResponse(
                x.Id,
                x.VenueId,
                x.Venue.Name,
                x.OrganizerId,
                x.Title,
                x.Description,
                x.StartDate,
                x.EndDate,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByTitleVenueAndStartDateAsync(string title, Guid venueId, DateTime startDate, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(
                x => x.Title == title &&
                     x.VenueId == venueId &&
                     x.StartDate == startDate,
                cancellationToken);
    }

    public async Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        await _context.Events.AddAsync(eventEntity, cancellationToken);
    }

    public async Task<bool> ExistsByTitleVenueAndStartDateExceptIdAsync(string title, Guid venueId, DateTime startDate, Guid excludedEventId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(
                x => x.Title == title &&
                     x.VenueId == venueId &&
                     x.StartDate == startDate &&
                     x.Id != excludedEventId,
                cancellationToken);
    }
}