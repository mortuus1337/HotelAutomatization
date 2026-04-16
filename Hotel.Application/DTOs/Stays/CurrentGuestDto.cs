using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Stays;

public class CurrentGuestDto
{
    public int StayId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int GuestId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public bool IsMain { get; set; }
}

