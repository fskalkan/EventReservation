using EventReservation.Application.Features.Venues.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAndCityAsync(string name, string city, CancellationToken cancellationToken = default);

    Task AddAsync(Venue venue, CancellationToken cancellationToken = default);

    Task<VenueResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VenueResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}