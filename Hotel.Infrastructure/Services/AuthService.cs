using Hotel.Application.Common;
using Hotel.Application.DTOs.Auth;
using Hotel.Application.Interfaces;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly HotelDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public AuthService(
        HotelDbContext dbContext,
        ITokenService tokenService,
        IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
            throw new ValidationException("Логин обязателен.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Пароль обязателен.");

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Неверный логин или пароль.");

        if (!user.IsActive)
            throw new UnauthorizedException("Пользователь деактивирован.");

        var isPasswordValid = _passwordService.Verify(user.PasswordHash, request.Password);

        if (!isPasswordValid)
            throw new UnauthorizedException("Неверный логин или пароль.");

        // Автоматический переход со старого plaintext-формата на hash
        if (!_passwordService.IsHashed(user.PasswordHash))
        {
            user.PasswordHash = _passwordService.Hash(request.Password);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var token = _tokenService.CreateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            FullName = user.FullName,
            RoleCode = user.RoleCode
        };
    }
}