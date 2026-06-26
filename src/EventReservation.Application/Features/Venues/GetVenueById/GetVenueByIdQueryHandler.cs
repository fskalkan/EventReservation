using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.GetVenueById;

public sealed class GetVenueByIdQueryHandler : IQueryHandler<GetVenueByIdQuery, VenueResponse>
{
    private readonly IVenueRepository _venueRepository;

    public GetVenueByIdQueryHandler(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }
    public async Task<VenueResponse> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetResponseByIdAsync(request.Id, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue not found.");
        }

        return venue;
    }
}

