using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Domain.Entities;

public class Reservation
{

    public int ReservationId { get; set; }


    public DateTimeOffset CreatedAt { get; set; }


    public int CreatedByUserId { get; set; }


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

  
    public int? MealPlanId { get; set; }

    public MealPlan? MealPlan { get; set; }

    public AppUser? CreatedByUser { get; set; } = null!;

    public ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();

    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
}