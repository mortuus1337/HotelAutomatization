using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Reservations;

public class CreateReservationDto
{
    public string? Source { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? Comment { get; set; }
    public DateTime PlannedCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal Prepayment { get; set; }
    public int? MealPlanId { get; set; }
    public List<int> RoomIds { get; set; } = new();
}
