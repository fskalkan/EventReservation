using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.BackgroundJobs;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Infrastructure.Authentication;
using EventReservation.Infrastructure.BackgroundJobs;
using EventReservation.Infrastructure.Persistence;
using EventReservation.Infrastructure.Persistence.Repositories;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventReservation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddHangfire(hangfireConfiguration =>
        {
            hangfireConfiguration.UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"));
        });

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IEventSeatRepository, EventSeatRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IReservationExpirationJob, ReservationExpirationJob>();
        services.AddScoped<IReservationExpirationScheduler, HangfireReservationExpirationScheduler>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}