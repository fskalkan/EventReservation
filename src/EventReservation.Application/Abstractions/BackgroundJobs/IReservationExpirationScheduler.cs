namespace EventReservation.Application.Abstractions.BackgroundJobs;

public interface IReservationExpirationScheduler
{
    void ScheduleExpiration(Guid reservationId, DateTime expiresAt);
}