using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Entities;

public class AppUser
{
    public int UserId { get; set; }
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleCode { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Reservation> CreatedReservations { get; set; } = new List<Reservation>();
    public ICollection<Stay> CreatedStays { get; set; } = new List<Stay>();
    public ICollection<StayOperation> StayOperations { get; set; } = new List<StayOperation>();
}
