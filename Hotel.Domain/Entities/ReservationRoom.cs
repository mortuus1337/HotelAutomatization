using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Domain.Entities;

public class ReservationRoom
{

    public int ReservationRoomId { get; set; }


    public int ReservationId { get; set; }


    public int RoomId { get; set; }

    public decimal? PricePerNight { get; set; }


    public Reservation Reservation { get; set; } = null!;


    public Room Room { get; set; } = null!;
}