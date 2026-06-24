using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservation.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<EventSeat> EventSeats => Set<EventSeat>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<ReservationSeat> ReservationSeats => Set<ReservationSeat>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Venue>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Seat>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Event>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<EventSeat>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Reservation>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<ReservationSeat>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(x => !x.IsDeleted);
    }
}