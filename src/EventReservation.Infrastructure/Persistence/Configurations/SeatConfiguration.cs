using EventReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventReservation.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VenueId)
            .IsRequired();

        builder.Property(x => x.Section)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Row)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new
        {
            x.VenueId,
            x.Section,
            x.Row,
            x.Number
        })
        .IsUnique();

        builder.Ignore(x => x.Label);
    }
}