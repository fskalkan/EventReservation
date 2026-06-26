using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Venues.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Features.Venues.CreateVenue;

public sealed class CreateVenueCommandHandler
    : ICommandHandler<CreateVenueCommand, VenueResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVenueCommandHandler(
        IVenueRepository venueRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VenueResponse> Handle(
        CreateVenueCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var venueExists = await _venueRepository.ExistsByNameAndCityAsync(
            command.Name,
            command.City,
            cancellationToken);

        if (venueExists)
        {
            throw new BadRequestException("Venue already exists in this city.");
        }

        var venue = new Venue(
            command.Name,
            command.City,
            command.Address,
            command.Capacity,
            _currentUserService.UserId.Value);

        await _venueRepository.AddAsync(venue, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VenueResponse(
            venue.Id,
            venue.Name,
            venue.City,
            venue.Address,
            venue.Capacity,
            venue.CreatedByUserId,
            venue.CreatedAt);
    }
}