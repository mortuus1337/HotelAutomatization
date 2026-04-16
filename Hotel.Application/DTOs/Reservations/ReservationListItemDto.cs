using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Reservations;

public class ReservationListItemDto
{
    public int ReservationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime PlannedCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal TotalPrice { get; set; }
}
