namespace Hotel.Application.DTOs.Stays;

public class StayOperationDto
{
    public int StayOperationId { get; set; }
    public int StayId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string? Comment { get; set; }
}

