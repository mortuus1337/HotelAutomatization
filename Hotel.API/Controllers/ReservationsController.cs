using System.Security.Claims;
using Hotel.Application.DTOs.Reservations;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IRoomAvailabilityService _roomAvailabilityService;

    public ReservationsController(
        IReservationService reservationService,
        IRoomAvailabilityService roomAvailabilityService)
    {
        _reservationService = reservationService;
        _roomAvailabilityService = roomAvailabilityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservationListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _reservationService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(
        [FromBody] CreateReservationDto request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var currentUserId))
            return Unauthorized();

        var result = await _reservationService.CreateAsync(request, currentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<ReservationDto>> Confirm(int id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.ConfirmAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(int id, CancellationToken cancellationToken)
    {
        var result = await _reservationService.CancelAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("available-rooms")]
    public async Task<ActionResult<List<AvailableRoomDto>>> GetAvailableRooms(
        [FromQuery] DateTime plannedCheckin,
        [FromQuery] DateTime plannedCheckout,
        CancellationToken cancellationToken)
    {
        var result = await _roomAvailabilityService.GetAvailableRoomsAsync(
            plannedCheckin,
            plannedCheckout,
            cancellationToken);

        return Ok(result);
    }
}