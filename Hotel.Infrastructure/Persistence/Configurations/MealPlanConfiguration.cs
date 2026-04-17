using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.ToTable("meal_plan");

        builder.HasKey(x => x.MealPlanId);

        builder.Property(x => x.MealPlanId).HasColumnName("meal_plan_id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.PricePerPersonPerDay).HasColumnName("price_per_person_per_day");
    }
}
