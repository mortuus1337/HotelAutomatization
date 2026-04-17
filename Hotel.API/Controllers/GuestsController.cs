using Hotel.Application.DTOs.Guests;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GuestDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _guestService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuestDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _guestService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GuestDto>> Create(
        [FromBody] CreateGuestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _guestService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GuestDto>> Update(
        int id,
        [FromBody] UpdateGuestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _guestService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }
}