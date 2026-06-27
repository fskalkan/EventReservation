using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Events.Common;

namespace EventReservation.Application.Features.Events.UpdateEvent;

public sealed class UpdateEventCommandHandler
    : ICommandHandler<UpdateEventCommand, EventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEventCommandHandler(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventResponse> Handle(UpdateEventCommand command, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(command.Id, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        var eventExists = await _eventRepository.ExistsByTitleVenueAndStartDateExceptIdAsync(
            command.Title,
            eventEntity.VenueId,
            command.StartDate,
            command.Id,
            cancellationToken);

        if (eventExists)
        {
            throw new BadRequestException("Event already exists for this venue and start date.");
        }

        eventEntity.UpdateDetails(
            command.Title,
            command.Description,
            command.StartDate,
            command.EndDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await _eventRepository.GetResponseByIdAsync(eventEntity.Id, cancellationToken);

        return response!;
    }
}