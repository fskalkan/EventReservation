using EventReservation.Application.Features.Events.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByTitleVenueAndStartDateAsync(string title, Guid venueId, DateTime startDate, CancellationToken cancellationToken = default);

    Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTitleVenueAndStartDateExceptIdAsync(string title, Guid venueId, DateTime startDate, Guid excludedEventId, CancellationToken cancellationToken = default);
}