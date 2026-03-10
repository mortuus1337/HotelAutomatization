using Hotel.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("room_type");

        builder.HasKey(x => x.RoomTypeId);

        builder.Property(x => x.RoomTypeId)
            .HasColumnName("room_type_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(x => x.Capacity)
            .HasColumnName("capacity")
            .IsRequired();

        builder.Property(x => x.BasePrice)
            .HasColumnName("base_price")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasCheckConstraint("ck_room_type_capacity_positive", "\"capacity\" > 0");
        builder.HasCheckConstraint("ck_room_type_base_price_non_negative", "\"base_price\" >= 0");
    }
}