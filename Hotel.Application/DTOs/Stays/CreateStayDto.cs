using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Stays;

public class CreateStayDto
{
    public int RoomId { get; set; }
    public DateTime PlannedCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public int? MealPlanId { get; set; }
    public string? Comment { get; set; }
    public List<int> GuestIds { get; set; } = new();
}

