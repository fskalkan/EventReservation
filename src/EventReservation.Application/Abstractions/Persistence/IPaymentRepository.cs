using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}