using Hotel.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.ToTable("meal_plan");

        builder.HasKey(x => x.MealPlanId);

        builder.Property(x => x.MealPlanId)
            .HasColumnName("meal_plan_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.PricePerPersonPerDay)
            .HasColumnName("price_per_person_per_day")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasCheckConstraint("ck_meal_plan_price_positive", "\"price_per_person_per_day\" >= 0");
    }
}