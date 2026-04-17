using Hotel.Application.Common;
using Hotel.Application.DTOs.Dashboard;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const decimal HousekeepingCostPerRoomNight = 350m;
    private const decimal UtilitiesCostPerActiveRoomPerDay = 120m;

    private readonly HotelDbContext _dbContext;
    
    private sealed class PeriodSnapshot
    {
        public int ReservationsCount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal ProfitOrLoss { get; set; }
        public decimal AverageLoadPercent { get; set; }
    }

    public DashboardService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OwnerDashboardDto> GetOwnerDashboardAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ValidateRange(from, to);

        var startDate = DateOnly.FromDateTime(from.Date);
        var endDateExclusive = DateOnly.FromDateTime(to.Date.AddDays(1));
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var activeRooms = await _dbContext.Rooms
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new
            {
                x.RoomId,
                x.RoomNumber
            })
            .ToListAsync(cancellationToken);

        var activeRoomIds = activeRooms.Select(x => x.RoomId).ToHashSet();

        var totalRooms = await _dbContext.Rooms
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var stays = await _dbContext.Stays
            .AsNoTracking()
            .Where(x =>
                activeRoomIds.Contains(x.RoomId) &&
                x.Status != StayStatuses.Cancelled &&
                x.PlannedCheckin < endDateExclusive &&
                x.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.StayId,
                x.RoomId,
                x.PlannedCheckin,
                x.PlannedCheckout,
                x.Status
            })
            .ToListAsync(cancellationToken);

        var reservationRooms = await _dbContext.ReservationRooms
            .AsNoTracking()
            .Where(x =>
                activeRoomIds.Contains(x.RoomId) &&
                (x.Reservation.Status == ReservationStatuses.Created ||
                 x.Reservation.Status == ReservationStatuses.Confirmed) &&
                x.Reservation.PlannedCheckin < endDateExclusive &&
                x.Reservation.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.ReservationId,
                x.RoomId,
                x.Room.RoomNumber,
                x.Reservation.CustomerName,
                x.Reservation.CustomerPhone,
                x.Reservation.Status,
                x.Reservation.PlannedCheckin,
                x.Reservation.PlannedCheckout
            })
            .ToListAsync(cancellationToken);

        var occupiedNowRoomIds = stays
            .Where(x => today >= x.PlannedCheckin && today < x.PlannedCheckout)
            .Select(x => x.RoomId)
            .Distinct()
            .ToList();

        var currentStayIds = await _dbContext.Stays
            .AsNoTracking()
            .Where(x =>
                x.Status == StayStatuses.Active &&
                today >= x.PlannedCheckin &&
                today < x.PlannedCheckout)
            .Select(x => x.StayId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var currentGuests = 0;
        if (currentStayIds.Count > 0)
        {
            currentGuests = await _dbContext.StayGuests
                .AsNoTracking()
                .Where(x => currentStayIds.Contains(x.StayId))
                .CountAsync(cancellationToken);
        }

        var reservationsGrouped = reservationRooms
            .GroupBy(x => new
            {
                x.ReservationId,
                x.CustomerName,
                x.CustomerPhone,
                x.Status,
                x.PlannedCheckin,
                x.PlannedCheckout
            })
            .Select(g => new ReservationSummaryDto
            {
                ReservationId = g.Key.ReservationId,
                CustomerName = g.Key.CustomerName ?? string.Empty,
                CustomerPhone = g.Key.CustomerPhone,
                Status = g.Key.Status,
                PlannedCheckin = g.Key.PlannedCheckin.ToDateTime(TimeOnly.MinValue),
                PlannedCheckout = g.Key.PlannedCheckout.ToDateTime(TimeOnly.MinValue),
                RoomNumbers = g.Select(x => x.RoomNumber).Distinct().OrderBy(x => x).ToList()
            })
            .OrderBy(x => x.PlannedCheckin)
            .ToList();

        var arrivalsDepartures = new List<ArrivalDepartureDto>();
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            var day = DateOnly.FromDateTime(date);

            arrivalsDepartures.Add(new ArrivalDepartureDto
            {
                Date = date,
                Arrivals = reservationRooms
                    .Where(x => x.PlannedCheckin == day)
                    .Select(x => x.ReservationId)
                    .Distinct()
                    .Count(),
                Departures = stays
                    .Where(x => x.PlannedCheckout == day)
                    .Select(x => x.StayId)
                    .Distinct()
                    .Count()
            });
        }

        var result = new OwnerDashboardDto
        {
            From = from.Date,
            To = to.Date,
            TotalRooms = totalRooms,
            ActiveRooms = activeRooms.Count,
            OccupiedNow = occupiedNowRoomIds.Count,
            CurrentStays = currentStayIds.Count,
            CurrentGuests = currentGuests,
            ReservationsInPeriod = reservationsGrouped.Count,
            UpcomingArrivals = reservationsGrouped.Count(x => x.PlannedCheckin.Date >= from.Date && x.PlannedCheckin.Date <= to.Date),
            UpcomingDepartures = arrivalsDepartures.Sum(x => x.Departures),
            OccupancyPercentNow = activeRooms.Count == 0
                ? 0
                : Math.Round(occupiedNowRoomIds.Count * 100m / activeRooms.Count, 2),
            Reservations = reservationsGrouped,
            ArrivalsDepartures = arrivalsDepartures
        };

        for (var date = startDate; date < endDateExclusive; date = date.AddDays(1))
        {
            var occupiedRooms = stays
                .Where(x => date >= x.PlannedCheckin && date < x.PlannedCheckout)
                .Select(x => x.RoomId)
                .Distinct()
                .ToHashSet();

            var reservedRooms = reservationRooms
                .Where(x => date >= x.PlannedCheckin && date < x.PlannedCheckout)
                .Select(x => x.RoomId)
                .Distinct()
                .Where(roomId => !occupiedRooms.Contains(roomId))
                .Count();

            result.DailyLoad.Add(new DashboardDailyLoadDto
            {
                Date = date.ToDateTime(TimeOnly.MinValue),
                OccupiedRooms = occupiedRooms.Count,
                ReservedRooms = reservedRooms
            });
        }

        var financeStays = await _dbContext.Stays
            .AsNoTracking()
            .Where(x =>
                activeRoomIds.Contains(x.RoomId) &&
                x.Status != StayStatuses.Cancelled &&
                x.PlannedCheckin < endDateExclusive &&
                x.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.StayId,
                x.Status,
                x.RoomId,
                x.ReservationId,
                x.PlannedCheckin,
                x.PlannedCheckout,
                BasePrice = x.Room.RoomType.BasePrice,
                ReservationSource = x.Reservation != null ? x.Reservation.Source : null,
                ReservationRoomPrice = x.Reservation != null
                    ? x.Reservation.ReservationRooms
                        .Where(rr => rr.RoomId == x.RoomId)
                        .Select(rr => rr.PricePerNight)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync(cancellationToken);

        var incomeBySource = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        decimal realizedIncome = 0m;
        decimal bookedIncome = 0m;
        var totalRoomNights = 0;

        foreach (var stay in financeStays)
        {
            var overlapStart = stay.PlannedCheckin > startDate ? stay.PlannedCheckin : startDate;
            var overlapEndExclusive = stay.PlannedCheckout < endDateExclusive ? stay.PlannedCheckout : endDateExclusive;
            var nights = overlapEndExclusive.DayNumber - overlapStart.DayNumber;

            if (nights <= 0)
                continue;

            totalRoomNights += nights;

            var nightlyRate = stay.ReservationRoomPrice ?? stay.BasePrice;
            var stayIncome = nightlyRate * nights;

            var rawSourceName = stay.ReservationId.HasValue
                ? (string.IsNullOrWhiteSpace(stay.ReservationSource) ? "Reservation" : stay.ReservationSource.Trim())
                : "FrontDeskCheckIn";

            var sourceName = NormalizeIncomeSource(rawSourceName, stay.ReservationId.HasValue);
            if (sourceName is null)
            {
                if (stay.Status == StayStatuses.Planned)
                    bookedIncome += stayIncome;
                else
                    realizedIncome += stayIncome;

                continue;
            }

            if (!incomeBySource.ContainsKey(sourceName))
                incomeBySource[sourceName] = 0m;
            incomeBySource[sourceName] += stayIncome;

            if (stay.Status == StayStatuses.Planned)
                bookedIncome += stayIncome;
            else
                realizedIncome += stayIncome;
        }

        var periodDays = (to.Date - from.Date).Days + 1;
        var housekeepingExpense = totalRoomNights * HousekeepingCostPerRoomNight;
        var utilitiesExpense = activeRooms.Count * periodDays * UtilitiesCostPerActiveRoomPerDay;
        var totalExpenses = housekeepingExpense + utilitiesExpense;
        var totalIncome = realizedIncome + bookedIncome;

        result.Finance = new FinanceSummaryDto
        {
            TotalIncome = Math.Round(totalIncome, 2),
            RealizedIncome = Math.Round(realizedIncome, 2),
            BookedIncome = Math.Round(bookedIncome, 2),
            TotalExpenses = Math.Round(totalExpenses, 2),
            ProfitOrLoss = Math.Round(totalIncome - totalExpenses, 2),
            IncomeBySource = incomeBySource
                .OrderByDescending(x => x.Value)
                .Select(x => new FinanceBreakdownItemDto
                {
                    Name = x.Key,
                    Amount = Math.Round(x.Value, 2)
                })
                .ToList(),
            ExpenseByCategory = new List<FinanceBreakdownItemDto>
            {
                new()
                {
                    Name = "Уборка и обслуживание номеров",
                    Amount = Math.Round(housekeepingExpense, 2)
                },
                new()
                {
                    Name = "Коммунальные услуги",
                    Amount = Math.Round(utilitiesExpense, 2)
                }
            }
        };

        var previousToDate = from.Date.AddDays(-1);
        var previousFromDate = previousToDate.AddDays(-periodDays + 1);
        var previousSnapshot = await BuildPeriodSnapshotAsync(
            DateOnly.FromDateTime(previousFromDate),
            DateOnly.FromDateTime(previousToDate.AddDays(1)),
            activeRoomIds,
            activeRooms.Count,
            cancellationToken);

        var currentAverageLoadPercent = activeRooms.Count == 0 || result.DailyLoad.Count == 0
            ? 0m
            : Math.Round(result.DailyLoad
                .Average(x => (x.OccupiedRooms + x.ReservedRooms) * 100m / activeRooms.Count), 2);

        result.PeriodComparison = new DashboardPeriodComparisonDto
        {
            PreviousFrom = previousFromDate,
            PreviousTo = previousToDate,
            ReservationsDelta = result.ReservationsInPeriod - previousSnapshot.ReservationsCount,
            ReservationsDeltaPercent = CalculateDeltaPercent(result.ReservationsInPeriod, previousSnapshot.ReservationsCount),
            TotalIncomeDelta = Math.Round(result.Finance.TotalIncome - previousSnapshot.TotalIncome, 2),
            TotalIncomeDeltaPercent = CalculateDeltaPercent(result.Finance.TotalIncome, previousSnapshot.TotalIncome),
            ProfitDelta = Math.Round(result.Finance.ProfitOrLoss - previousSnapshot.ProfitOrLoss, 2),
            ProfitDeltaPercent = CalculateDeltaPercent(result.Finance.ProfitOrLoss, previousSnapshot.ProfitOrLoss),
            AverageLoadPercentCurrent = currentAverageLoadPercent,
            AverageLoadPercentPrevious = previousSnapshot.AverageLoadPercent,
            AverageLoadDeltaPercentPoints = Math.Round(currentAverageLoadPercent - previousSnapshot.AverageLoadPercent, 2)
        };

        return result;
    }

    private async Task<PeriodSnapshot> BuildPeriodSnapshotAsync(
        DateOnly startDate,
        DateOnly endDateExclusive,
        HashSet<int> activeRoomIds,
        int activeRoomsCount,
        CancellationToken cancellationToken)
    {
        var stays = await _dbContext.Stays
            .AsNoTracking()
            .Where(x =>
                activeRoomIds.Contains(x.RoomId) &&
                x.Status != StayStatuses.Cancelled &&
                x.PlannedCheckin < endDateExclusive &&
                x.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.StayId,
                x.RoomId,
                x.PlannedCheckin,
                x.PlannedCheckout,
                x.Status
            })
            .ToListAsync(cancellationToken);

        var reservationRooms = await _dbContext.ReservationRooms
            .AsNoTracking()
            .Where(x =>
                activeRoomIds.Contains(x.RoomId) &&
                (x.Reservation.Status == ReservationStatuses.Created ||
                 x.Reservation.Status == ReservationStatuses.Confirmed) &&
                x.Reservation.PlannedCheckin < endDateExclusive &&
                x.Reservation.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.ReservationId,
                x.RoomId,
                x.Reservation.PlannedCheckin,
                x.Reservation.PlannedCheckout
            })
            .ToListAsync(cancellationToken);

        var financeStays = await _dbContext.Stays
            .AsNoTracking()
            .Where(x =>
                activeRoomIds.Contains(x.RoomId) &&
                x.Status != StayStatuses.Cancelled &&
                x.PlannedCheckin < endDateExclusive &&
                x.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.Status,
                x.RoomId,
                x.ReservationId,
                x.PlannedCheckin,
                x.PlannedCheckout,
                BasePrice = x.Room.RoomType.BasePrice,
                ReservationRoomPrice = x.Reservation != null
                    ? x.Reservation.ReservationRooms
                        .Where(rr => rr.RoomId == x.RoomId)
                        .Select(rr => rr.PricePerNight)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync(cancellationToken);

        decimal realizedIncome = 0m;
        decimal bookedIncome = 0m;
        var totalRoomNights = 0;

        foreach (var stay in financeStays)
        {
            var overlapStart = stay.PlannedCheckin > startDate ? stay.PlannedCheckin : startDate;
            var overlapEndExclusive = stay.PlannedCheckout < endDateExclusive ? stay.PlannedCheckout : endDateExclusive;
            var nights = overlapEndExclusive.DayNumber - overlapStart.DayNumber;

            if (nights <= 0)
                continue;

            totalRoomNights += nights;

            var nightlyRate = stay.ReservationRoomPrice ?? stay.BasePrice;
            var stayIncome = nightlyRate * nights;

            if (stay.Status == StayStatuses.Planned)
                bookedIncome += stayIncome;
            else
                realizedIncome += stayIncome;
        }

        var periodDays = endDateExclusive.DayNumber - startDate.DayNumber;
        decimal totalLoadPercent = 0m;

        for (var date = startDate; date < endDateExclusive; date = date.AddDays(1))
        {
            var occupiedRooms = stays
                .Where(x => date >= x.PlannedCheckin && date < x.PlannedCheckout)
                .Select(x => x.RoomId)
                .Distinct()
                .ToHashSet();

            var reservedRooms = reservationRooms
                .Where(x => date >= x.PlannedCheckin && date < x.PlannedCheckout)
                .Select(x => x.RoomId)
                .Distinct()
                .Where(roomId => !occupiedRooms.Contains(roomId))
                .Count();

            if (activeRoomsCount > 0)
                totalLoadPercent += (occupiedRooms.Count + reservedRooms) * 100m / activeRoomsCount;
        }

        var housekeepingExpense = totalRoomNights * HousekeepingCostPerRoomNight;
        var utilitiesExpense = activeRoomsCount * periodDays * UtilitiesCostPerActiveRoomPerDay;
        var totalExpenses = housekeepingExpense + utilitiesExpense;
        var totalIncome = realizedIncome + bookedIncome;

        return new PeriodSnapshot
        {
            ReservationsCount = reservationRooms.Select(x => x.ReservationId).Distinct().Count(),
            TotalIncome = Math.Round(totalIncome, 2),
            TotalExpenses = Math.Round(totalExpenses, 2),
            ProfitOrLoss = Math.Round(totalIncome - totalExpenses, 2),
            AverageLoadPercent = periodDays <= 0
                ? 0m
                : Math.Round(totalLoadPercent / periodDays, 2)
        };
    }

    private static decimal CalculateDeltaPercent(decimal current, decimal previous)
    {
        if (previous == 0m)
            return current == 0m ? 0m : 100m;

        return Math.Round((current - previous) * 100m / Math.Abs(previous), 2);
    }

    private static void ValidateRange(DateTime from, DateTime to)
    {
        if (from == default)
            throw new ValidationException("Дата начала периода обязательна.");

        if (to == default)
            throw new ValidationException("Дата окончания периода обязательна.");

        if (to.Date < from.Date)
            throw new ValidationException("Дата окончания не может быть раньше даты начала.");

        if ((to.Date - from.Date).TotalDays > 366)
            throw new ValidationException("Период не должен превышать 366 дней.");
    }

    private static string? NormalizeIncomeSource(string rawSource, bool isReservationSource)
    {
        var normalized = rawSource.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
            return isReservationSource ? "Прочие источники" : "Прямое заселение (стойка)";

        if (IsTechnicalSource(normalized))
            return null;

        if (normalized.Contains("frontdeskcheckin")
            || normalized.Contains("walkin")
            || normalized.Contains("front desk checkin"))
            return "Прямое заселение (стойка)";

        if (normalized.Contains("reception")
            || normalized.Contains("ресепш")
            || normalized.Contains("стойк")
            || normalized == "frontdesk")
            return "Стойка регистрации";

        if (normalized.Contains("phone")
            || normalized.Contains("телефон")
            || normalized.Contains("call"))
            return "Телефонные бронирования";

        if (normalized.Contains("website")
            || normalized.Contains("site")
            || normalized.Contains("сайт")
            || normalized.Contains("web"))
            return "Сайт гостиницы";

        if (normalized.Contains("ota")
            || normalized.Contains("booking")
            || normalized.Contains("aggregator")
            || normalized.Contains("agoda")
            || normalized.Contains("ostrovok")
            || normalized.Contains("expedia")
            || normalized.Contains("travel"))
            return "Онлайн-агрегаторы (OTA)";

        if (normalized.Contains("reservation"))
            return "Прочие источники";

        return "Прочие источники";
    }

    private static bool IsTechnicalSource(string normalizedSource)
    {
        return normalizedSource is "qa" or "test" or "autotest" or "demo" or "debug" or "seed"
            || normalizedSource.StartsWith("test_")
            || normalizedSource.StartsWith("qa_")
            || normalizedSource.Contains("seed")
            || normalizedSource.Contains("dummy")
            || normalizedSource.Contains("mock");
    }
}
