using System;
using System.Collections.Generic;
using System.Text;

namespace Hotel.Domain.Entities;

public class MealPlan
{
    public int MealPlanId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PricePerPersonPerDay { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
}