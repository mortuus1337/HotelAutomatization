using Hotel.Application.Common;
using Hotel.Application.DTOs.Calendar;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class CalendarService : ICalendarService
{
    private readonly HotelDbContext _dbContext;

    public CalendarService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomCalendarDto>> GetRoomCalendarAsync(
        DateTime from,
        DateTime to,
        int? roomTypeId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRange(from, to);

        var startDate = DateOnly.FromDateTime(from.Date);
        var endDateExclusive = DateOnly.FromDateTime(to.Date.AddDays(1));

        var roomsQuery = _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .Where(x => x.IsActive);

        if (roomTypeId.HasValue)
            roomsQuery = roomsQuery.Where(x => x.RoomTypeId == roomTypeId.Value);

        var rooms = await roomsQuery
            .OrderBy(x => x.RoomNumber)
            .ToListAsync(cancellationToken);

        var roomIds = rooms.Select(x => x.RoomId).ToList();

        var stays = await _dbContext.Stays
            .AsNoTracking()
            .Where(x =>
                roomIds.Contains(x.RoomId) &&
                x.Status != StayStatuses.Cancelled &&
                x.PlannedCheckin < endDateExclusive &&
                x.PlannedCheckout > startDate)
            .Select(x => new
            {
                x.StayId,
                x.RoomId,
                x.PlannedCheckin,
                x.PlannedCheckout
            })
            .ToListAsync(cancellationToken);

        var reservations = await _dbContext.ReservationRooms
            .AsNoTracking()
            .Where(x =>
                roomIds.Contains(x.RoomId) &&
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

        var result = new List<RoomCalendarDto>();

        foreach (var room in rooms)
        {
            var roomCalendar = new RoomCalendarDto
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType.Name,
                Floor = room.Floor
            };

            var roomStays = stays.Where(x => x.RoomId == room.RoomId).ToList();
            var roomReservations = reservations.Where(x => x.RoomId == room.RoomId).ToList();

            for (var date = startDate; date < endDateExclusive; date = date.AddDays(1))
            {
                var stay = roomStays.FirstOrDefault(x =>
                    date >= x.PlannedCheckin && date < x.PlannedCheckout);

                if (stay is not null)
                {
                    roomCalendar.Days.Add(new RoomCalendarDayDto
                    {
                        Date = date.ToDateTime(TimeOnly.MinValue),
                        Status = "Occupied",
                        StayId = stay.StayId
                    });

                    continue;
                }

                var reservation = roomReservations.FirstOrDefault(x =>
                    date >= x.PlannedCheckin && date < x.PlannedCheckout);

                if (reservation is not null)
                {
                    roomCalendar.Days.Add(new RoomCalendarDayDto
                    {
                        Date = date.ToDateTime(TimeOnly.MinValue),
                        Status = "Reserved",
                        ReservationId = reservation.ReservationId
                    });

                    continue;
                }

                roomCalendar.Days.Add(new RoomCalendarDayDto
                {
                    Date = date.ToDateTime(TimeOnly.MinValue),
                    Status = "Free"
                });
            }

            result.Add(roomCalendar);
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
