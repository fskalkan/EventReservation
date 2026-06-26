using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAndCityAsync(string name, string city, CancellationToken cancellationToken = default);

    Task AddAsync(Venue venue, CancellationToken cancellationToken = default);
}