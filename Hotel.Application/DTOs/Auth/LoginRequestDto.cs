using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
