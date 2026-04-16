using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Entities;

public class GuestIdentity
{
    public int GuestId { get; set; }
    public string DocType { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public string? IssuedBy { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Citizenship { get; set; }
    public string? Address { get; set; }

    public Guest Guest { get; set; } = null!;
}
