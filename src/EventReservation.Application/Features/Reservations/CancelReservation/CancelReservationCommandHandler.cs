using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Reservations.CancelReservation;

public sealed class CancelReservationCommandHandler
    : ICommandHandler<CancelReservationCommand, ReservationResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelReservationCommandHandler(
        ICurrentUserService currentUserService,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResponse> Handle(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var reservation = await _reservationRepository.GetByIdWithDetailsAsync(command.ReservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reservation not found.");
        }

        if (reservation.CustomerId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You cannot cancel another customer's reservation.");
        }

        if (reservation.Status != ReservationStatus.PendingPayment)
        {
            throw new BadRequestException("Only pending payment reservations can be cancelled.");
        }

        reservation.Cancel();

        foreach (var reservationSeat in reservation.ReservationSeats)
        {
            reservationSeat.EventSeat.Release();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var seats = reservation.ReservationSeats
            .Select(x => new ReservationSeatResponse(
                x.EventSeatId,
                x.EventSeat.Seat.Label,
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
}