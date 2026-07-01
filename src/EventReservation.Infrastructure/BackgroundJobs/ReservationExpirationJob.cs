using EventReservation.Application.Abstractions.BackgroundJobs;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Abstractions.Realtime;
using EventReservation.Domain.Enums;

namespace EventReservation.Infrastructure.BackgroundJobs;

public sealed class ReservationExpirationJob : IReservationExpirationJob
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public ReservationExpirationJob(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IRealtimeNotifier realtimeNotifier)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task ExpireAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetByIdWithDetailsAsync(reservationId, CancellationToken.None);

        if (reservation is null)
        {
            return;
        }

        if (reservation.Status != ReservationStatus.PendingPayment)
        {
            return;
        }

        if (reservation.ExpiresAt > DateTime.UtcNow)
        {
            return;
        }

        reservation.Expire();

        foreach (var reservationSeat in reservation.ReservationSeats)
        {
            reservationSeat.EventSeat.Release();
        }

        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        await _realtimeNotifier.NotifyEventSeatsChangedAsync(
            reservation.EventId,
            reservation.ReservationSeats
                .Select(reservationSeat =>
                {
                    var eventSeat = reservationSeat.EventSeat;

                    return new EventSeatStatusChangedMessage(
                        eventSeat.EventId,
                        eventSeat.Id,
                        eventSeat.SeatId,
                        eventSeat.Seat.Label,
                        eventSeat.Price,
                        eventSeat.Status);
                })
                .ToList(),
            CancellationToken.None);
    }
}