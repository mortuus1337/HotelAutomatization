using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Rooms;

public class RoomType
{
    public long RoomTypeId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public string? Description { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
