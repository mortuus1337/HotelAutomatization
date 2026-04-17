using System.ComponentModel.DataAnnotations;

namespace Hotel.Application.DTOs.Reservations;

public class CreateReservationDto
{
    [MaxLength(100, ErrorMessage = "Источник не должен превышать 100 символов.")]
    public string? Source { get; set; }

    [Required(ErrorMessage = "Имя заказчика обязательно.")]
    [MaxLength(200, ErrorMessage = "Имя заказчика не должно превышать 200 символов.")]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Телефон не должен превышать 50 символов.")]
    public string? CustomerPhone { get; set; }

    [MaxLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Дата заезда обязательна.")]
    public DateTime PlannedCheckin { get; set; }

    [Required(ErrorMessage = "Дата выезда обязательна.")]
    public DateTime PlannedCheckout { get; set; }

    [Range(0, 20, ErrorMessage = "Количество взрослых должно быть от 0 до 20.")]
    public int Adults { get; set; }

    [Range(0, 20, ErrorMessage = "Количество детей должно быть от 0 до 20.")]
    public int Children { get; set; }

    [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Предоплата не может быть отрицательной.")]
    public decimal Prepayment { get; set; }

    public int? MealPlanId { get; set; }

    [MinLength(1, ErrorMessage = "Нужно выбрать хотя бы один номер.")]
    public List<int> RoomIds { get; set; } = new();
}
