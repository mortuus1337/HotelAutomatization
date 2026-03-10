using Hotel.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class ReservationRoomConfiguration : IEntityTypeConfiguration<ReservationRoom>
{
    public void Configure(EntityTypeBuilder<ReservationRoom> builder)
    {
        builder.ToTable("reservation_room");

        builder.HasKey(x => x.ReservationRoomId);

        builder.Property(x => x.ReservationRoomId)
            .HasColumnName("reservation_room_id");

        builder.Property(x => x.PricePerNight)
            .HasColumnName("price_per_night")
            .HasPrecision(12, 2);

        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.Rooms)
            .HasForeignKey(x => x.ReservationId);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId);
    }
}