using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Seats.Common;
using EventReservation.Domain.Entities;

namespace EventReservation.Application.Features.Seats.GenerateVenueSeats;

public sealed class GenerateVenueSeatsCommandHandler
    : ICommandHandler<GenerateVenueSeatsCommand, IReadOnlyList<SeatResponse>>
{
    private readonly IVenueRepository _venueRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateVenueSeatsCommandHandler(
        IVenueRepository venueRepository,
        ISeatRepository seatRepository,
        IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _seatRepository = seatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SeatResponse>> Handle(GenerateVenueSeatsCommand command, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetByIdAsync(command.VenueId, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue not found.");
        }

        var seatsToCreateCount = command.EndNumber - command.StartNumber + 1;

        var currentSeatCount = await _seatRepository.CountByVenueIdAsync(command.VenueId, cancellationToken);

        if (currentSeatCount + seatsToCreateCount > venue.Capacity)
        {
            throw new BadRequestException("Seat count exceeds venue capacity.");
        }

        var seatRangeExists = await _seatRepository.ExistsInRangeAsync(
            command.VenueId,
            command.Section,
            command.Row,
            command.StartNumber,
            command.EndNumber,
            cancellationToken);

        if (seatRangeExists)
        {
            throw new BadRequestException("Seat range already exists for this venue.");
        }

        var seats = Enumerable
            .Range(command.StartNumber, seatsToCreateCount)
            .Select(number => new Seat(
                command.VenueId,
                command.Section,
                command.Row,
                number))
            .ToList();

        await _seatRepository.AddRangeAsync(seats, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return seats
            .Select(seat => new SeatResponse(
                seat.Id,
                seat.VenueId,
                seat.Section,
                seat.Row,
                seat.Number,
                seat.Label,
                seat.CreatedAt))
            .ToList();
    }
}