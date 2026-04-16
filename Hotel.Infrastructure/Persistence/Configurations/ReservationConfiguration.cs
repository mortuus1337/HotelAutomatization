using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation");

        builder.HasKey(x => x.ReservationId);

        builder.Property(x => x.ReservationId).HasColumnName("reservation_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Source).HasColumnName("source");
        builder.Property(x => x.CustomerName).HasColumnName("customer_name");
        builder.Property(x => x.CustomerPhone).HasColumnName("customer_phone");
        builder.Property(x => x.Comment).HasColumnName("comment");
        builder.Property(x => x.PlannedCheckin).HasColumnName("planned_checkin");
        builder.Property(x => x.PlannedCheckout).HasColumnName("planned_checkout");
        builder.Property(x => x.Adults).HasColumnName("adults");
        builder.Property(x => x.Children).HasColumnName("children");
        builder.Property(x => x.TotalPrice).HasColumnName("total_price");
        builder.Property(x => x.Prepayment).HasColumnName("prepayment");
        builder.Property(x => x.MealPlanId).HasColumnName("meal_plan_id");

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedReservations)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MealPlan)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.MealPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
