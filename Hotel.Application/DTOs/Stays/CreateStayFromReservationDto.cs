using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Stays;

public class CreateStayFromReservationDto
{
    public int ReservationId { get; set; }
    public string? Comment { get; set; }
}

