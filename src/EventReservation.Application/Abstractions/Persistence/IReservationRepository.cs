using EventReservation.Application.Features.Reservations.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ReservationResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReservationResponse>> GetResponsesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}