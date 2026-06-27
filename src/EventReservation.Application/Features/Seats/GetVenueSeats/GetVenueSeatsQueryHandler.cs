using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Seats.Common;

namespace EventReservation.Application.Features.Seats.GetVenueSeats;

public sealed class GetVenueSeatsQueryHandler
    : IQueryHandler<GetVenueSeatsQuery, IReadOnlyList<SeatResponse>>
{
    private readonly IVenueRepository _venueRepository;
    private readonly ISeatRepository _seatRepository;

    public GetVenueSeatsQueryHandler(
        IVenueRepository venueRepository,
        ISeatRepository seatRepository)
    {
        _venueRepository = venueRepository;
        _seatRepository = seatRepository;
    }

    public async Task<IReadOnlyList<SeatResponse>> Handle(GetVenueSeatsQuery query, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetByIdAsync(query.VenueId, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue not found.");
        }

        return await _seatRepository.GetResponsesByVenueIdAsync(query.VenueId, cancellationToken);
    }
}