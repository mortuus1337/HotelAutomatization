using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Entities;

public class Guest
{
    public int GuestId { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public GuestIdentity? GuestIdentity { get; set; }
    public ICollection<StayGuest> StayGuests { get; set; } = new List<StayGuest>();
}
