using Hotel.Application.DTOs.Reservations;
using Hotel.Domain.Constants;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Services;
using Hotel.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hotel.Tests.Services;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesReservation_WithCalculatedTotalPrice()
    {
        await using var db = TestDbContextFactory.Create();

        db.AppUsers.Add(new AppUser
        {
            UserId = 1,
            Login = "admin",
            PasswordHash = "hash",
            FullName = "Admin User",
            RoleCode = "Admin",
            IsActive = true
        });

        db.RoomTypes.Add(new RoomType
        {
            RoomTypeId = 1,
            Name = "Standard",
            Capacity = 2,
            BasePrice = 3000m
        });

        db.Rooms.Add(new Room
        {
            RoomId = 1,
            RoomNumber = "101",
            RoomTypeId = 1,
            Floor = 1,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var availabilityService = new RoomAvailabilityService(db);
        var service = new ReservationService(db, availabilityService, NullLogger<ReservationService>.Instance);

        var request = new CreateReservationDto
        {
            CustomerName = "Иван Иванов",
            CustomerPhone = "+7-900-000-00-00",
            PlannedCheckin = new DateTime(2026, 4, 20),
            PlannedCheckout = new DateTime(2026, 4, 23),
            Adults = 2,
            Children = 0,
            Prepayment = 1000m,
            RoomIds = new List<int> { 1 }
        };

        var result = await service.CreateAsync(request, currentUserId: 1);

        Assert.Equal(ReservationStatuses.Created, result.Status);
        Assert.Single(result.Rooms);
        Assert.Equal(9000m, result.TotalPrice);

        var reservationRoomCount = db.ReservationRooms.Count();
        Assert.Equal(1, reservationRoomCount);
    }
}
