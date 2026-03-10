using Hotel.Domain.Identity;
using Hotel.Domain.Rooms;

namespace Hotel.Domain.Reservations;

public class Stay
{
    public long StayId { get; set; }

    public long? ReservationId { get; set; }

    public long RoomId { get; set; }

    public string Status { get; set; } = null!;

    public DateTimeOffset? ActualCheckin { get; set; }

    public DateTimeOffset? ActualCheckout { get; set; }

    public DateOnly PlannedCheckin { get; set; }

    public DateOnly PlannedCheckout { get; set; }

    public long? MealPlanId { get; set; }

    public long? CreatedByUserId { get; set; }

    public string? Comment { get; set; }

    public Reservation? Reservation { get; set; }

    public Room Room { get; set; } = null!;

    public MealPlan? MealPlan { get; set; }

    public AppUser? CreatedByUser { get; set; }

    public ICollection<StayGuest> Guests { get; set; } = new List<StayGuest>();
}