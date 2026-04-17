using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Calendar;

public class RoomCalendarDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Floor { get; set; }
    public List<RoomCalendarDayDto> Days { get; set; } = new();
}

public class RoomCalendarDayDto
{
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty; // Free, Reserved, Occupied
    public int? ReservationId { get; set; }
    public int? StayId { get; set; }
}
