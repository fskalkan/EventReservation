using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;

namespace EventReservation.Application.Features.Venues.DeleteVenue;

public sealed class DeleteVenueCommandHandler : ICommandHandler<DeleteVenueCommand, DeleteVenueResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVenueCommandHandler(
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteVenueResponse> Handle(DeleteVenueCommand command, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetByIdAsync(command.Id, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue not found.");
        }

        venue.SoftDelete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteVenueResponse(true);
    }
}