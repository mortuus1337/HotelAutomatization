using Hotel.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class StayConfiguration : IEntityTypeConfiguration<Stay>
{
    public void Configure(EntityTypeBuilder<Stay> builder)
    {
        builder.ToTable("stay");

        builder.HasKey(x => x.StayId);

        builder.Property(x => x.StayId)
            .HasColumnName("stay_id");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.PlannedCheckin)
            .HasColumnName("planned_checkin");

        builder.Property(x => x.PlannedCheckout)
            .HasColumnName("planned_checkout");

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId);

        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.ReservationId);

        builder.HasCheckConstraint(
            "ck_stay_dates",
            "\"planned_checkout\" > \"planned_checkin\""
        );
    }
}