using EventReservation.Application.Abstractions.Messaging;

namespace EventReservation.Application.Features.Venues.DeleteVenue;

public sealed record DeleteVenueCommand(Guid Id) : ICommand<DeleteVenueResponse>;