using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Rooms;

public class RoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Floor { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
