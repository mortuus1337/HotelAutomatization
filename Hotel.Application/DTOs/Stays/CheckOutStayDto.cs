using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Stays;

public class CheckOutStayDto
{
    public DateTimeOffset? ActualCheckout { get; set; }
    public string? Comment { get; set; }
}

