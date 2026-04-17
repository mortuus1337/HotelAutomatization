using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Guests;

public class UpdateGuestDto
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? DocType { get; set; }
    public string? DocNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Citizenship { get; set; }
    public string? Address { get; set; }
}

