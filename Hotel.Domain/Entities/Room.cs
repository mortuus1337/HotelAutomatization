using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Entities;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public int Floor { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    public RoomType RoomType { get; set; } = null!;
    public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
}

