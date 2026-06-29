using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.EventReports.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.EventReports.GetEventReservationSummary;

public sealed class GetEventReservationSummaryQueryHandler
    : IQueryHandler<GetEventReservationSummaryQuery, EventReservationSummaryResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventRepository _eventRepository;
    private readonly IEventReportRepository _eventReportRepository;

    public GetEventReservationSummaryQueryHandler(
        ICurrentUserService currentUserService,
        IEventRepository eventRepository,
        IEventReportRepository eventReportRepository)
    {
        _currentUserService = currentUserService;
        _eventRepository = eventRepository;
        _eventReportRepository = eventReportRepository;
    }

    public async Task<EventReservationSummaryResponse> Handle(GetEventReservationSummaryQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var eventEntity = await _eventRepository.GetByIdAsync(query.EventId, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        var isAdmin = string.Equals(
            _currentUserService.Role,
            UserRole.Admin.ToString(),
            StringComparison.OrdinalIgnoreCase);

        var isOrganizer = string.Equals(
            _currentUserService.Role,
            UserRole.Organizer.ToString(),
            StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && (!isOrganizer || eventEntity.OrganizerId != _currentUserService.UserId.Value))
        {
            throw new ForbiddenAccessException("You cannot view reports for this event.");
        }

        return await _eventReportRepository.GetSummaryByEventIdAsync(query.EventId, cancellationToken);
    }
}