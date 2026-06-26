using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.GetVenues;

public sealed class GetVenuesQueryHandler : IQueryHandler<GetVenuesQuery, IReadOnlyList<VenueResponse>>
{

    private readonly IVenueRepository _venueRepository;

    public GetVenuesQueryHandler(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }
    public async Task<IReadOnlyList<VenueResponse>> Handle(GetVenuesQuery request, CancellationToken cancellationToken)
    {
        return await _venueRepository.GetAllAsync(cancellationToken);
    }
}
