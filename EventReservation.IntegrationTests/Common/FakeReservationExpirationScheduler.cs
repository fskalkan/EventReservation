using EventReservation.Application.Abstractions.BackgroundJobs;

namespace EventReservation.IntegrationTests.Common;

public sealed class FakeReservationExpirationScheduler : IReservationExpirationScheduler
{
    public List<(Guid ReservationId, DateTime ExpiresAt)> ScheduledJobs { get; } = new();

    public void ScheduleExpiration(Guid reservationId, DateTime expiresAt)
    {
        ScheduledJobs.Add((reservationId, expiresAt));
    }
}