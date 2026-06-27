using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Events.Common;

public sealed record EventResponse(
    Guid Id,
    Guid VenueId,
    string VenueName,
    Guid OrganizerId,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    EventStatus Status,
    DateTime CreatedAt);