namespace EventReservation.Application.Abstractions.BackgroundJobs;

public interface IReservationExpirationJob
{
    Task ExpireAsync(Guid reservationId);
}