using EventReservation.Application.Abstractions.BackgroundJobs;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Domain.Enums;

namespace EventReservation.Infrastructure.BackgroundJobs;

public sealed class ReservationExpirationJob : IReservationExpirationJob
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationExpirationJob(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
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
    }
}