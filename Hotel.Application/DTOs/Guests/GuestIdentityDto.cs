using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Guests;

public class GuestIdentityDto
{
    public int GuestId { get; set; }
    public string? DocType { get; set; }
    public string? DocNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Citizenship { get; set; }
    public string? Address { get; set; }
}
