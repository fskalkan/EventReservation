using EventReservation.Application.Abstractions.BackgroundJobs;
using Hangfire;

namespace EventReservation.Infrastructure.BackgroundJobs;

public sealed class HangfireReservationExpirationScheduler : IReservationExpirationScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireReservationExpirationScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void ScheduleExpiration(Guid reservationId, DateTime expiresAt)
    {
        var delay = expiresAt - DateTime.UtcNow;

        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        _backgroundJobClient.Schedule<IReservationExpirationJob>(
            job => job.ExpireAsync(reservationId),
            delay);
    }
}