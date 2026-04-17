using System.ComponentModel.DataAnnotations;

namespace Hotel.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Логин обязателен.")]
    [MaxLength(100, ErrorMessage = "Логин не должен превышать 100 символов.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен.")]
    [MaxLength(200, ErrorMessage = "Пароль не должен превышать 200 символов.")]
    public string Password { get; set; } = string.Empty;
}
