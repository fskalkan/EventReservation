using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Events.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Events.CancelEvent;

public sealed class CancelEventCommandHandler
    : ICommandHandler<CancelEventCommand, EventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventResponse> Handle(CancelEventCommand command, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(command.Id, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        if (eventEntity.Status == EventStatus.Cancelled)
        {
            throw new BadRequestException("Event is already cancelled.");
        }

        if (eventEntity.Status == EventStatus.Completed)
        {
            throw new BadRequestException("Completed events cannot be cancelled.");
        }

        eventEntity.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await _eventRepository.GetResponseByIdAsync(eventEntity.Id, cancellationToken);

        return response!;
    }
}