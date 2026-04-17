namespace Hotel.Application.DTOs.Reservations;

public class ReservationDto
{
    public int ReservationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? Comment { get; set; }
    public DateTime PlannedCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Prepayment { get; set; }
    public int? MealPlanId { get; set; }
    public string? MealPlanName { get; set; }
    public List<ReservationRoomDto> Rooms { get; set; } = new();
}