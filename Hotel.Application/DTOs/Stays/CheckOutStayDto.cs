using System.ComponentModel.DataAnnotations;

namespace Hotel.Application.DTOs.Stays;

public class CheckOutStayDto
{
    public DateTimeOffset? ActualCheckout { get; set; }

    [MaxLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
    public string? Comment { get; set; }
}
