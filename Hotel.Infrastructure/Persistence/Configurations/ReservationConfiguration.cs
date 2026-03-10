using Hotel.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation");

        builder.HasKey(x => x.ReservationId);

        builder.Property(x => x.ReservationId)
            .HasColumnName("reservation_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.PlannedCheckin)
            .HasColumnName("planned_checkin");

        builder.Property(x => x.PlannedCheckout)
            .HasColumnName("planned_checkout");

        builder.Property(x => x.Adults)
            .HasColumnName("adults");

        builder.Property(x => x.Children)
            .HasColumnName("children");

        builder.Property(x => x.TotalPrice)
            .HasColumnName("total_price")
            .HasPrecision(12, 2);

        builder.Property(x => x.Prepayment)
            .HasColumnName("prepayment")
            .HasPrecision(12, 2);

        builder.HasOne(x => x.MealPlan)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.MealPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "ck_reservation_dates",
            "\"planned_checkout\" > \"planned_checkin\""
        );

        builder.HasCheckConstraint(
            "ck_reservation_adults",
            "\"adults\" >= 1"
        );
    }
}