using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Rooms;

public class Room
{
    public long RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public long RoomTypeId { get; set; }
    public int? Floor { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public RoomType RoomType { get; set; } = null!;
}
