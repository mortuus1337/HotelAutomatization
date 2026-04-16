using Hotel.Application.Common;
using Hotel.Application.DTOs.Dashboard;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly HotelDbContext _dbContext;

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

        return result;
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
}
