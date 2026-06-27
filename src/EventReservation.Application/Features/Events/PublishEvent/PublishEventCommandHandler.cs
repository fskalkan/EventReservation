using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Events.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Events.PublishEvent;

public sealed class PublishEventCommandHandler
    : ICommandHandler<PublishEventCommand, EventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventResponse> Handle(PublishEventCommand command, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(command.Id, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        if (eventEntity.Status != EventStatus.Draft)
        {
            throw new BadRequestException("Only draft events can be published.");
        }

        eventEntity.Publish();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await _eventRepository.GetResponseByIdAsync(eventEntity.Id, cancellationToken);

        return response!;
    }
}