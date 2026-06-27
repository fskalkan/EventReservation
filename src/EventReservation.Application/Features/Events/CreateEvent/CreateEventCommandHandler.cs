using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Events.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Features.Events.CreateEvent;

public sealed class CreateEventCommandHandler
    : ICommandHandler<CreateEventCommand, EventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEventCommandHandler(
        IEventRepository eventRepository,
        IVenueRepository venueRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _venueRepository = venueRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventResponse> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var venue = await _venueRepository.GetByIdAsync(command.VenueId, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue not found.");
        }

        var eventExists = await _eventRepository.ExistsByTitleVenueAndStartDateAsync(command.Title, command.VenueId, command.StartDate, cancellationToken);

        if (eventExists)
        {
            throw new BadRequestException("Event already exists for this venue and start date.");
        }

        var eventEntity = new Event(
            command.VenueId,
            _currentUserService.UserId.Value,
            command.Title,
            command.Description,
            command.StartDate,
            command.EndDate);

        await _eventRepository.AddAsync(eventEntity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventResponse(
            eventEntity.Id,
            eventEntity.VenueId,
            venue.Name,
            eventEntity.OrganizerId,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.StartDate,
            eventEntity.EndDate,
            eventEntity.Status,
            eventEntity.CreatedAt);
    }
}