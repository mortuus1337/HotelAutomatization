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
    public FinanceSummaryDto Finance { get; set; } = new();
    public DashboardPeriodComparisonDto PeriodComparison { get; set; } = new();

    public List<DashboardDailyLoadDto> DailyLoad { get; set; } = new();
    public List<ReservationSummaryDto> Reservations { get; set; } = new();
    public List<ArrivalDepartureDto> ArrivalsDepartures { get; set; } = new();
}

public class DashboardPeriodComparisonDto
{
    public DateTime PreviousFrom { get; set; }
    public DateTime PreviousTo { get; set; }

    public int ReservationsDelta { get; set; }
    public decimal ReservationsDeltaPercent { get; set; }

    public decimal TotalIncomeDelta { get; set; }
    public decimal TotalIncomeDeltaPercent { get; set; }

    public decimal ProfitDelta { get; set; }
    public decimal ProfitDeltaPercent { get; set; }

    public decimal AverageLoadPercentCurrent { get; set; }
    public decimal AverageLoadPercentPrevious { get; set; }
    public decimal AverageLoadDeltaPercentPoints { get; set; }
}

public class FinanceSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal RealizedIncome { get; set; }
    public decimal BookedIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal ProfitOrLoss { get; set; }

    public List<FinanceBreakdownItemDto> IncomeBySource { get; set; } = new();
    public List<FinanceBreakdownItemDto> ExpenseByCategory { get; set; } = new();
}

public class FinanceBreakdownItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
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
