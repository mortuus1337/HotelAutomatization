using System.ComponentModel.DataAnnotations;

namespace Hotel.Application.DTOs.Stays;

public class CreateStayDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Нужно выбрать номер.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Дата заезда обязательна.")]
    public DateTime PlannedCheckin { get; set; }

    [Required(ErrorMessage = "Дата выезда обязательна.")]
    public DateTime PlannedCheckout { get; set; }

    public int? MealPlanId { get; set; }

    [MaxLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
    public string? Comment { get; set; }

    [MinLength(1, ErrorMessage = "Нужно указать хотя бы одного гостя.")]
    public List<int> GuestIds { get; set; } = new();
}
