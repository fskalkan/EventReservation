using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.CreateVenue;

    public sealed record CreateVenueCommand(
        string Name,
        string City,
        string Address,
        int Capacity) : ICommand<VenueResponse>;

