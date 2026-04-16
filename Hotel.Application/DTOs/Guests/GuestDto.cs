using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Guests;

public class GuestDto
{
    public int GuestId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public GuestIdentityDto? Identity { get; set; }
}

