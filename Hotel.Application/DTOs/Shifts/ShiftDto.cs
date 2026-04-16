namespace Hotel.Application.DTOs.Shifts;

public class ShiftDto
{
    public int WorkShiftId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public class OpenShiftDto
{
    public string? Comment { get; set; }
}

public class CloseShiftDto
{
    public string? Comment { get; set; }
}
