using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}