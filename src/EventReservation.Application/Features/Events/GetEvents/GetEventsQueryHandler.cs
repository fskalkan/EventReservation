using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.GetEvents;

public sealed class GetEventsQueryHandler : IQueryHandler<GetEventsQuery, IReadOnlyList<EventResponse>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<EventResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        return await _eventRepository.GetAllAsync(cancellationToken);
    }
}

