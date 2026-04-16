namespace Hotel.Domain.Entities;

public class StayOperation
{
    public int StayOperationId { get; set; }
    public int StayId { get; set; }
    public int UserId { get; set; }
    public string OperationType { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }
    public string? Comment { get; set; }

    public Stay Stay { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}

