using Hotel.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("room");

        builder.HasKey(x => x.RoomId);

        builder.Property(x => x.RoomId)
            .HasColumnName("room_id");

        builder.Property(x => x.RoomNumber)
            .HasColumnName("room_number")
            .IsRequired();

        builder.Property(x => x.RoomTypeId)
            .HasColumnName("room_type_id")
            .IsRequired();

        builder.Property(x => x.Floor)
            .HasColumnName("floor");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.Notes)
            .HasColumnName("notes");

        builder.HasIndex(x => x.RoomNumber)
            .IsUnique();

        builder.HasOne(x => x.RoomType)
            .WithMany(x => x.Rooms)
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}