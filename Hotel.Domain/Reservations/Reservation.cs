using Hotel.Domain.Identity;

namespace Hotel.Domain.Reservations;

public class Reservation
{
    public long ReservationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public long? CreatedByUserId { get; set; }

    public string Status { get; set; } = null!;

    public string? Source { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? Comment { get; set; }

    public DateOnly PlannedCheckin { get; set; }

    public DateOnly PlannedCheckout { get; set; }

    public int Adults { get; set; }

    public int Children { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? Prepayment { get; set; }

    public long? MealPlanId { get; set; }

    public MealPlan? MealPlan { get; set; }

    public AppUser? CreatedByUser { get; set; }

    public ICollection<ReservationRoom> Rooms { get; set; } = new List<ReservationRoom>();

    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
}