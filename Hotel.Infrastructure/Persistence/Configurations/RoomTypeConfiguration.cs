using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("room_type");

        builder.HasKey(x => x.RoomTypeId);

        builder.Property(x => x.RoomTypeId).HasColumnName("room_type_id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Capacity).HasColumnName("capacity");
        builder.Property(x => x.BasePrice).HasColumnName("base_price");
        builder.Property(x => x.Description).HasColumnName("description");
    }
}
