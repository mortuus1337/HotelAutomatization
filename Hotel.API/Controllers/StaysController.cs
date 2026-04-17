using System.Security.Claims;
using Hotel.Application.DTOs.Stays;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class StaysController : ControllerBase
{
    private readonly IStayService _stayService;

    public StaysController(IStayService stayService)
    {
        _stayService = stayService;
    }

    [HttpGet("current")]
    public async Task<ActionResult<List<CurrentStayDto>>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _stayService.GetCurrentAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("operations")]
    public async Task<ActionResult<List<StayOperationDto>>> GetOperations(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        var result = await _stayService.GetOperationsAsync(from, to, userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StayDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _stayService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<StayDto>> CheckIn(
        [FromBody] CreateStayDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        var result = await _stayService.CheckInAsync(request, currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("check-in-by-reservation")]
    public async Task<ActionResult<StayDto>> CheckInByReservation(
        [FromBody] CreateStayFromReservationDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        var result = await _stayService.CheckInByReservationAsync(request, currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/check-out")]
    public async Task<ActionResult<StayDto>> CheckOut(
        int id,
        [FromBody] CheckOutStayDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        var result = await _stayService.CheckOutAsync(id, request, currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("current-guests")]
    public async Task<ActionResult<List<CurrentGuestDto>>> GetCurrentGuests(CancellationToken cancellationToken)
    {
        var result = await _stayService.GetCurrentGuestsAsync(cancellationToken);
        return Ok(result);
    }
}
