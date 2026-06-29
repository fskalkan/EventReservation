using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.Reservations.PayReservation;

public sealed class PayReservationCommandHandler
    : ICommandHandler<PayReservationCommand, PayReservationResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PayReservationCommandHandler(
        ICurrentUserService currentUserService,
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayReservationResponse> Handle(PayReservationCommand command, CancellationToken cancellationToken)
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
            throw new ForbiddenAccessException("You cannot pay another customer's reservation.");
        }

        if (reservation.Status != ReservationStatus.PendingPayment)
        {
            throw new BadRequestException("Only pending payment reservations can be paid.");
        }

        if (reservation.Payment is not null)
        {
            throw new BadRequestException("Payment already exists for this reservation.");
        }

        if (reservation.ExpiresAt <= DateTime.UtcNow)
        {
            reservation.Expire();

            foreach (var reservationSeat in reservation.ReservationSeats)
            {
                reservationSeat.EventSeat.Release();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new BadRequestException("Reservation has expired.");
        }

        if (command.Amount != reservation.TotalAmount)
        {
            throw new BadRequestException("Payment amount does not match reservation total amount.");
        }

        var payment = new Payment(
            reservation.Id,
            reservation.TotalAmount,
            command.Method);

        payment.MarkAsSuccess();

        reservation.Confirm();

        foreach (var reservationSeat in reservation.ReservationSeats)
        {
            reservationSeat.EventSeat.Reserve();
        }

        await _paymentRepository.AddAsync(payment, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayReservationResponse(
            reservation.Id,
            reservation.ReservationCode,
            reservation.Status,
            payment.Id,
            payment.Status,
            payment.Amount,
            payment.Method,
            payment.PaidAt);
    }
}