namespace Hotel.Application.DTOs.Dashboard;

public class OwnerDashboardDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public int TotalRooms { get; set; }
    public int ActiveRooms { get; set; }

    public int OccupiedNow { get; set; }
    public int CurrentStays { get; set; }
    public int CurrentGuests { get; set; }

    public int ReservationsInPeriod { get; set; }
    public int UpcomingArrivals { get; set; }
    public int UpcomingDepartures { get; set; }

    public decimal OccupancyPercentNow { get; set; }

    public List<DashboardDailyLoadDto> DailyLoad { get; set; } = new();
    public List<ReservationSummaryDto> Reservations { get; set; } = new();
    public List<ArrivalDepartureDto> ArrivalsDepartures { get; set; } = new();
}

public class DashboardDailyLoadDto
{
    public DateTime Date { get; set; }
    public int OccupiedRooms { get; set; }
    public int ReservedRooms { get; set; }
}

public class ReservationSummaryDto
{
    public int ReservationId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public List<string> RoomNumbers { get; set; } = new();
    public DateTime PlannedCheckin { get; set; }
    public DateTime PlannedCheckout { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ArrivalDepartureDto
{
    public DateTime Date { get; set; }
    public int Arrivals { get; set; }
    public int Departures { get; set; }
}
