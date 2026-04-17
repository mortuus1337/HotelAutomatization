using System.Security.Claims;
using Hotel.Application.DTOs.Shifts;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class ShiftsController : ControllerBase
{
    private readonly IWorkShiftService _workShiftService;

    public ShiftsController(IWorkShiftService workShiftService)
    {
        _workShiftService = workShiftService;
    }

    private int GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Некорректный токен пользователя.");

        return userId;
    }

    [HttpGet("current")]
    public async Task<ActionResult<ShiftDto?>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _workShiftService.GetCurrentShiftAsync(GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<ShiftDto?>> GetActive(CancellationToken cancellationToken)
    {
        var result = await _workShiftService.GetActiveShiftAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("report")]
    public async Task<ActionResult<ShiftReportDto>> GetReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        var result = await _workShiftService.GetShiftReportAsync(
            from,
            to,
            userId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("open")]
    public async Task<ActionResult<ShiftDto>> Open(
        [FromBody] OpenShiftDto request,
        CancellationToken cancellationToken)
    {
        var result = await _workShiftService.OpenShiftAsync(
            GetCurrentUserId(),
            request.Comment,
            request.TakeoverIfNeeded,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("close")]
    public async Task<ActionResult<ShiftDto>> Close(
        [FromBody] CloseShiftDto request,
        CancellationToken cancellationToken)
    {
        var result = await _workShiftService.CloseShiftAsync(
            GetCurrentUserId(),
            request.Comment,
            cancellationToken);

        return Ok(result);
    }
}
