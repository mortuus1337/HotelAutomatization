namespace Hotel.Application.DTOs.Shifts;

public class ShiftReportDto
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int? UserId { get; set; }
    public int TotalShifts { get; set; }
    public int OpenShifts { get; set; }
    public int ClosedShifts { get; set; }
    public double TotalDurationMinutes { get; set; }
    public int TotalActionsCount { get; set; }
    public int TotalCheckInCount { get; set; }
    public int TotalCheckOutCount { get; set; }
    public List<ShiftReportItemDto> Items { get; set; } = new();
}

public class ShiftReportItemDto
{
    public int WorkShiftId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public double DurationMinutes { get; set; }
    public int ActionsCount { get; set; }
    public int CheckInCount { get; set; }
    public int CheckOutCount { get; set; }
    public DateTimeOffset? LastActionAt { get; set; }
    public List<ShiftActionDto> Actions { get; set; } = new();
}

public class ShiftActionDto
{
    public int StayOperationId { get; set; }
    public int StayId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string? Comment { get; set; }
}
