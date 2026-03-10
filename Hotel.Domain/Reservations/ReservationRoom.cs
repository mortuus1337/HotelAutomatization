using Hotel.Domain.Rooms;

namespace Hotel.Domain.Reservations;

public class ReservationRoom
{
    public long ReservationRoomId { get; set; }

    public long ReservationId { get; set; }

    public long RoomId { get; set; }

    public decimal? PricePerNight { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public Room Room { get; set; } = null!;
}