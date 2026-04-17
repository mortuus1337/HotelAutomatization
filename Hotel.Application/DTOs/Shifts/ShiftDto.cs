using System.ComponentModel.DataAnnotations;

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
    [MaxLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов.")]
    public string? Comment { get; set; }

    public bool TakeoverIfNeeded { get; set; }
}

public class CloseShiftDto
{
    [MaxLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов.")]
    public string? Comment { get; set; }
}
