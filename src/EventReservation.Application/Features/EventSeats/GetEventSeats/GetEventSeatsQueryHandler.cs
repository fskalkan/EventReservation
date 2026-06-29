using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.EventSeats.Common;

namespace EventReservation.Application.Features.EventSeats.GetEventSeats;

public sealed class GetEventSeatsQueryHandler
    : IQueryHandler<GetEventSeatsQuery, IReadOnlyList<EventSeatResponse>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventSeatRepository _eventSeatRepository;

    public GetEventSeatsQueryHandler(
        IEventRepository eventRepository,
        IEventSeatRepository eventSeatRepository)
    {
        _eventRepository = eventRepository;
        _eventSeatRepository = eventSeatRepository;
    }

    public async Task<IReadOnlyList<EventSeatResponse>> Handle(GetEventSeatsQuery query, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(query.EventId, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        return await _eventSeatRepository.GetResponsesByEventIdAsync(query.EventId, cancellationToken);
    }
}