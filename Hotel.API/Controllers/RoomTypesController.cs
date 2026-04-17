using Hotel.Application.DTOs.Rooms;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypesController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Owner}")]
    public async Task<ActionResult<List<RoomTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roomTypeService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RoomTypeDto>> Create(
        [FromBody] CreateRoomTypeDto request,
        CancellationToken cancellationToken)
    {
        var result = await _roomTypeService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }
}