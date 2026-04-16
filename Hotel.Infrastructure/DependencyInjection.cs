using Hotel.Application.Interfaces;
using Hotel.Infrastructure.Auth;
using Hotel.Infrastructure.Persistence;
using Hotel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddDbContext<HotelDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IPasswordService, PasswordService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IRoomService, RoomService>();

        services.AddScoped<IRoomAvailabilityService, RoomAvailabilityService>();
        services.AddScoped<IReservationService, ReservationService>();

        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IStayService, StayService>();

        services.AddScoped<IWorkShiftService, WorkShiftService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICalendarService, CalendarService>();

        return services;
    }
}
