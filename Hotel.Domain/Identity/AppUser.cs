using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Identity;

public class AppUser
{
    public long UserId { get; set; }
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleCode { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}