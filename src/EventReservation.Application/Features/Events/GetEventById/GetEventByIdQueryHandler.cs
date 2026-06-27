using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.GetEventById;

public sealed class GetEventByIdQueryHandler : IQueryHandler<GetEventByIdQuery, EventResponse>
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventResponse> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var eventResponse = await _eventRepository.GetResponseByIdAsync(query.Id, cancellationToken);

        if (eventResponse is null)
        {
            throw new NotFoundException("Event not found.");
        }

        return eventResponse;
    }
}