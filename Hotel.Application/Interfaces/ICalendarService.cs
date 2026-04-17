using Hotel.Application.DTOs.Calendar;

namespace Hotel.Application.Interfaces;

public interface ICalendarService
{
    Task<List<RoomCalendarDto>> GetRoomCalendarAsync(
        DateTime from,
        DateTime to,
        int? roomTypeId = null,
        CancellationToken cancellationToken = default);
}
