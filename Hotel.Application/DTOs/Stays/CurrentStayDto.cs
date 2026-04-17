using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Stays;

public class CurrentStayDto
{
    public int StayId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ActualCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public string? Comment { get; set; }
}

