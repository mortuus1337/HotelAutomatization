using Hotel.Application.Common;
using Hotel.Application.DTOs.Auth;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Auth;
using Hotel.Infrastructure.Services;
using Hotel.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hotel.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ReturnsToken_ForValidCredentials()
    {
        await using var db = TestDbContextFactory.Create();
        var passwordService = new PasswordService();

        db.AppUsers.Add(new AppUser
        {
            UserId = 1,
            Login = "admin",
            PasswordHash = passwordService.Hash("admin123"),
            FullName = "Admin User",
            RoleCode = "Admin",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var tokenService = new TokenService(Options.Create(new JwtSettings
        {
            Issuer = "hotel-tests",
            Audience = "hotel-tests",
            SecretKey = "super_secret_key_for_tests_1234567890",
            ExpiresMinutes = 60
        }));

        var service = new AuthService(db, tokenService, passwordService, NullLogger<AuthService>.Instance);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Login = "admin",
            Password = "admin123"
        });

        Assert.Equal(1, result.UserId);
        Assert.Equal("Admin", result.RoleCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_ForWrongPassword()
    {
        await using var db = TestDbContextFactory.Create();
        var passwordService = new PasswordService();

        db.AppUsers.Add(new AppUser
        {
            UserId = 1,
            Login = "admin",
            PasswordHash = passwordService.Hash("admin123"),
            FullName = "Admin User",
            RoleCode = "Admin",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var tokenService = new TokenService(Options.Create(new JwtSettings
        {
            Issuer = "hotel-tests",
            Audience = "hotel-tests",
            SecretKey = "super_secret_key_for_tests_1234567890",
            ExpiresMinutes = 60
        }));

        var service = new AuthService(db, tokenService, passwordService, NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequestDto
            {
                Login = "admin",
                Password = "wrong"
            }));
    }
}
