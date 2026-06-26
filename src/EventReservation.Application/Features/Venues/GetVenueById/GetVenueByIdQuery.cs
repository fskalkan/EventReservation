using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.GetVenueById;
    public sealed record GetVenueByIdQuery(Guid Id) : IQuery<VenueResponse>;


