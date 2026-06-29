using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.BackgroundJobs;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.Common;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Reservations.CreateReservation;

public sealed class CreateReservationCommandHandler
    : ICommandHandler<CreateReservationCommand, ReservationResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventRepository _eventRepository;
    private readonly IEventSeatRepository _eventSeatRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservationExpirationScheduler _reservationExpirationScheduler;

    public CreateReservationCommandHandler(
        ICurrentUserService currentUserService,
        IEventRepository eventRepository,
        IEventSeatRepository eventSeatRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IReservationExpirationScheduler reservationExpirationScheduler)
    {
        _currentUserService = currentUserService;
        _eventRepository = eventRepository;
        _eventSeatRepository = eventSeatRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _reservationExpirationScheduler = reservationExpirationScheduler;
    }

    public async Task<ReservationResponse> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var eventEntity = await _eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        if (eventEntity.Status != EventStatus.Published)
        {
            throw new BadRequestException("Reservations can only be created for published events.");
        }

        var requestedSeatIds = command.EventSeatIds
            .Distinct()
            .ToList();

        var eventSeats = await _eventSeatRepository.GetByIdsAsync(requestedSeatIds, cancellationToken);

        if (eventSeats.Count != requestedSeatIds.Count)
        {
            throw new NotFoundException("One or more event seats were not found.");
        }

        var hasSeatFromDifferentEvent = eventSeats.Any(x => x.EventId != command.EventId);

        if (hasSeatFromDifferentEvent)
        {
            throw new BadRequestException("All selected seats must belong to the requested event.");
        }

        var hasUnavailableSeat = eventSeats.Any(x => x.Status != EventSeatStatus.Available);

        if (hasUnavailableSeat)
        {
            throw new BadRequestException("One or more selected seats are not available.");
        }

        var reservation = new Reservation(
            GenerateReservationCode(),
            _currentUserService.UserId.Value,
            eventEntity.Id,
            DateTime.UtcNow.AddMinutes(10));

        foreach (var eventSeat in eventSeats)
        {
            eventSeat.Lock();

            reservation.AddSeat(
                eventSeat.Id,
                eventSeat.Price);
        }

        await _reservationRepository.AddAsync(reservation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _reservationExpirationScheduler.ScheduleExpiration(reservation.Id, reservation.ExpiresAt);

        var seats = eventSeats
            .Select(x => new ReservationSeatResponse(
                x.Id,
                x.Seat.Label,
                x.Price))
            .ToList();

        return new ReservationResponse(
            reservation.Id,
            reservation.ReservationCode,
            reservation.EventId,
            reservation.CustomerId,
            reservation.Status,
            reservation.TotalAmount,
            reservation.ExpiresAt,
            reservation.CreatedAt,
            seats);
    }

    private static string GenerateReservationCode()
    {
        return $"RSV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }
}