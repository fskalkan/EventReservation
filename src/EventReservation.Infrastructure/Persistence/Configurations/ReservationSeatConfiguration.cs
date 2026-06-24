using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventReservation.Infrastructure.Persistence.Configurations;

public class ReservationSeatConfiguration : IEntityTypeConfiguration<ReservationSeat>
{
    public void Configure(EntityTypeBuilder<ReservationSeat> builder)
    {
        builder.ToTable("ReservationSeats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservationId)
            .IsRequired();

        builder.Property(x => x.EventSeatId)
            .IsRequired();

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new
        {
            x.ReservationId,
            x.EventSeatId
        })
        .IsUnique();

        builder.HasIndex(x => x.ReservationId);

        builder.HasIndex(x => x.EventSeatId);
    }
}