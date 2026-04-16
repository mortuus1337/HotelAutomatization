using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Rooms;

public class CreateRoomTypeDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public string? Description { get; set; }
}
