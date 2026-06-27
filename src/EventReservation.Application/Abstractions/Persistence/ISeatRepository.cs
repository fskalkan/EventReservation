using EventReservation.Application.Features.Seats.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Abstractions.Persistence;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SeatResponse?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeatResponse>> GetResponsesByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<int> CountByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<bool> ExistsInRangeAsync(Guid venueId, string section, string row, int startNumber, int endNumber, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default);
}