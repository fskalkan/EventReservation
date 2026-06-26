using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.UpdateVenue;

public sealed record UpdateVenueCommand(
    Guid Id,
    string Name,
    string City,
    string Address,
    int Capacity) : ICommand<VenueResponse>;
