using System.ComponentModel.DataAnnotations;

namespace Hotel.Application.DTOs.Stays;

public class CreateStayFromReservationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Нужно указать корректную бронь.")]
    public int ReservationId { get; set; }

    [MaxLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
    public string? Comment { get; set; }
}
