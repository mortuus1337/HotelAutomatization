using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Domain.Entities;

public class Stay
{

    public int StayId { get; set; }


    public int? ReservationId { get; set; }


    public int RoomId { get; set; }


    public string Status { get; set; } = null!;

    public DateTimeOffset? ActualCheckin { get; set; }

    public DateTimeOffset? ActualCheckout { get; set; }

    public DateOnly PlannedCheckin { get; set; }

    public DateOnly PlannedCheckout { get; set; }

    public int? MealPlanId { get; set; }

    public int? CreatedByUserId { get; set; }

    public string? Comment { get; set; }

    public Reservation? Reservation { get; set; }

    public Room Room { get; set; } = null!;

    public MealPlan? MealPlan { get; set; }

    public AppUser? CreatedByUser { get; set; } = null!;

    public ICollection<StayGuest> StayGuests { get; set; } = new List<StayGuest>();
    public ICollection<StayOperation> Operations { get; set; } = new List<StayOperation>();
}
