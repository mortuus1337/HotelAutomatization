using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Reservations;

public class ReservationRoomDto
{
    public int ReservationRoomId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
}
