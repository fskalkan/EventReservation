using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Venues.Common;

namespace EventReservation.Application.Features.Venues.UpdateVenue;

public class UpdateVenueCommandHandler : ICommandHandler<UpdateVenueCommand, VenueResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateVenueCommandHandler(
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<VenueResponse> Handle(UpdateVenueCommand command, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetByIdAsync(command.Id, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue not found.");
        }
        
        var venueExists = await _venueRepository.ExistsByNameAndCityExceptIdAsync(
            command.Name,
            command.City,
            command.Id,
            cancellationToken);

        if (venueExists) 
        {
            throw new BadRequestException("Venue already exists in this city.");
        }

        venue.UpdateDetails(
            command.Name,
            command.City,
            command.Address,
            command.Capacity);

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

