using Hotel.Application.DTOs.Calendar;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Owner}")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet("rooms")]
    public async Task<ActionResult<List<RoomCalendarDto>>> GetRoomsCalendar(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? roomTypeId,
        CancellationToken cancellationToken)
    {
        var result = await _calendarService.GetRoomCalendarAsync(
            from,
            to,
            roomTypeId,
            cancellationToken);

        return Ok(result);
    }
}
