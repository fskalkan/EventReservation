using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.GetVenues;

public sealed record GetVenuesQuery : IQuery<IReadOnlyList<VenueResponse>>;