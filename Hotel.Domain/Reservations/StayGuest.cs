using Hotel.Domain.Guests;

namespace Hotel.Domain.Reservations;

public class StayGuest
{
    public long StayId { get; set; }

    public long GuestId { get; set; }

    public bool IsMain { get; set; }

    public Stay Stay { get; set; } = null!;

    public Guest Guest { get; set; } = null!;
}