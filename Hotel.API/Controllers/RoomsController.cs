using Hotel.Application.DTOs.Rooms;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Owner}")]
    public async Task<ActionResult<List<RoomDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roomService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Owner}")]
    public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _roomService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RoomDto>> Create(
        [FromBody] CreateRoomDto request,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RoomDto>> Update(
        int id,
        [FromBody] UpdateRoomDto request,
        CancellationToken cancellationToken)
    {
        var result = await _roomService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }
}