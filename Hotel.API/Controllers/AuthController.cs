using System.Security.Claims;
using Hotel.Application.DTOs.Auth;
using Hotel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var login = User.FindFirstValue(ClaimTypes.Name);
        var fullName = User.FindFirstValue("fullName");
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            UserId = userId,
            Login = login,
            FullName = fullName,
            Role = role
        });
    }
}
