using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
    }
}
