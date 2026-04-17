using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Stays;

public class StayDto
{
    public int StayId { get; set; }
    public int? ReservationId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ActualCheckin { get; set; }
    public DateTimeOffset? ActualCheckout { get; set; }
    public DateTime PlannedCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public int? MealPlanId { get; set; }
    public string? MealPlanName { get; set; }
    public int CreatedByUserId { get; set; }
    public string? Comment { get; set; }
    public List<StayGuestDto> Guests { get; set; } = new();
}

