using Hotel.Application.Common;
using Hotel.Application.DTOs.Reservations;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class RoomAvailabilityService : IRoomAvailabilityService
{
    private readonly HotelDbContext _dbContext;

    public RoomAvailabilityService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime plannedCheckin,
        DateTime plannedCheckout,
        CancellationToken cancellationToken = default)
    {
        return IsRoomAvailableAsync(roomId, plannedCheckin, plannedCheckout, null, cancellationToken);
    }

    public async Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime plannedCheckin,
        DateTime plannedCheckout,
        int? excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(plannedCheckin, plannedCheckout);

        var roomExists = await _dbContext.Rooms
            .AnyAsync(x => x.RoomId == roomId && x.IsActive, cancellationToken);

        if (!roomExists)
            throw new ValidationException("Указанный номер не найден или неактивен.");

        var checkinDate = DateOnly.FromDateTime(plannedCheckin);
        var checkoutDate = DateOnly.FromDateTime(plannedCheckout);

        var reservationQuery = _dbContext.ReservationRooms
            .Include(x => x.Reservation)
            .Where(x =>
                x.RoomId == roomId &&
                (x.Reservation.Status == ReservationStatuses.Created ||
                 x.Reservation.Status == ReservationStatuses.Confirmed) &&
                checkinDate < x.Reservation.PlannedCheckout &&
                checkoutDate > x.Reservation.PlannedCheckin);

        if (excludeReservationId.HasValue)
        {
            reservationQuery = reservationQuery
                .Where(x => x.ReservationId != excludeReservationId.Value);
        }

        var hasReservationConflict = await reservationQuery.AnyAsync(cancellationToken);

        if (hasReservationConflict)
            return false;

        var hasStayConflict = await _dbContext.Stays
            .AnyAsync(x =>
                x.RoomId == roomId &&
                (x.Status == StayStatuses.Planned ||
                 x.Status == StayStatuses.Active) &&
                checkinDate < x.PlannedCheckout &&
                checkoutDate > x.PlannedCheckin,
                cancellationToken);

        return !hasStayConflict;
    }

    public async Task<List<AvailableRoomDto>> GetAvailableRoomsAsync(
        DateTime plannedCheckin,
        DateTime plannedCheckout,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(plannedCheckin, plannedCheckout);

        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoomNumber)
            .ToListAsync(cancellationToken);

        var result = new List<AvailableRoomDto>();

        foreach (var room in rooms)
        {
            var isAvailable = await IsRoomAvailableAsync(
                room.RoomId,
                plannedCheckin,
                plannedCheckout,
                cancellationToken);

            if (!isAvailable)
                continue;

            result.Add(new AvailableRoomDto
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType.Name,
                Floor = room.Floor,
                BasePrice = room.RoomType.BasePrice
            });
        }

        return result;
    }

    private static void ValidateDates(DateTime plannedCheckin, DateTime plannedCheckout)
    {
        if (plannedCheckin == default)
            throw new ValidationException("Дата заезда обязательна.");

        if (plannedCheckout == default)
            throw new ValidationException("Дата выезда обязательна.");

        if (plannedCheckout <= plannedCheckin)
            throw new ValidationException("Дата выезда должна быть позже даты заезда.");
    }
}