using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Entities;

public class WorkShift
{
    public int WorkShiftId { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Status { get; set; } = null!;
    public string? Comment { get; set; }

    public AppUser User { get; set; } = null!;
}
