using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;

namespace EventReservation.Application.Features.Events.DeleteEvent;

public sealed class DeleteEventCommandHandler
    : ICommandHandler<DeleteEventCommand, DeleteEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteEventResponse> Handle(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(command.Id, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        eventEntity.SoftDelete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteEventResponse(true);
    }
}