using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class ReservationRoomConfiguration : IEntityTypeConfiguration<ReservationRoom>
{
    public void Configure(EntityTypeBuilder<ReservationRoom> builder)
    {
        builder.ToTable("reservation_room");

        builder.HasKey(x => x.ReservationRoomId);

        builder.Property(x => x.ReservationRoomId).HasColumnName("reservation_room_id");
        builder.Property(x => x.ReservationId).HasColumnName("reservation_id");
        builder.Property(x => x.RoomId).HasColumnName("room_id");
        builder.Property(x => x.PricePerNight).HasColumnName("price_per_night");

        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.ReservationRooms)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Room)
            .WithMany(x => x.ReservationRooms)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
