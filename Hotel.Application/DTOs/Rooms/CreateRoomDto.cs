using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Rooms;

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public int Floor { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
