using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class StayConfiguration : IEntityTypeConfiguration<Stay>
{
    public void Configure(EntityTypeBuilder<Stay> builder)
    {
        builder.ToTable("stay");

        builder.HasKey(x => x.StayId);

        builder.Property(x => x.StayId).HasColumnName("stay_id");
        builder.Property(x => x.ReservationId).HasColumnName("reservation_id");
        builder.Property(x => x.RoomId).HasColumnName("room_id");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.ActualCheckin).HasColumnName("actual_checkin");
        builder.Property(x => x.ActualCheckout).HasColumnName("actual_checkout");
        builder.Property(x => x.PlannedCheckin).HasColumnName("planned_checkin");
        builder.Property(x => x.PlannedCheckout).HasColumnName("planned_checkout");
        builder.Property(x => x.MealPlanId).HasColumnName("meal_plan_id");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.Comment).HasColumnName("comment");

        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MealPlan)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.MealPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedStays)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
