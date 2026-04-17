using Hotel.Application.DTOs.Stays;
using Hotel.Domain.Constants;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Services;
using Hotel.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hotel.Tests.Services;

public class StayServiceTests
{
    [Fact]
    public async Task CheckInAndCheckOut_UpdatesStayStatus_AndWritesOperations()
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
            BasePrice = 2500m
        });

        db.Rooms.Add(new Room
        {
            RoomId = 1,
            RoomNumber = "101",
            RoomTypeId = 1,
            Floor = 1,
            IsActive = true
        });

        db.Guests.Add(new Guest
        {
            GuestId = 1,
            LastName = "Иванов",
            FirstName = "Иван",
            GuestIdentity = new GuestIdentity
            {
                GuestId = 1,
                DocType = "Паспорт",
                DocNumber = "1234 567890",
                IssuedBy = "ОВД",
                IssuedDate = new DateOnly(2020, 1, 1),
                BirthDate = new DateOnly(1990, 5, 1),
                Citizenship = "RU",
                Address = "Новосибирск"
            }
        });

        await db.SaveChangesAsync();

        var availabilityService = new RoomAvailabilityService(db);
        var service = new StayService(db, availabilityService, NullLogger<StayService>.Instance);

        var checkInResult = await service.CheckInAsync(new CreateStayDto
        {
            RoomId = 1,
            PlannedCheckin = new DateTime(2026, 4, 20),
            PlannedCheckout = new DateTime(2026, 4, 22),
            GuestIds = new List<int> { 1 },
            Comment = "Тестовое заселение"
        }, currentUserId: 1);

        Assert.Equal(StayStatuses.Active, checkInResult.Status);
        Assert.Single(checkInResult.Guests);

        var checkOutResult = await service.CheckOutAsync(
            checkInResult.StayId,
            new CheckOutStayDto { Comment = "Тестовое выселение" },
            currentUserId: 1);

        Assert.Equal(StayStatuses.CheckedOut, checkOutResult.Status);
        Assert.NotNull(checkOutResult.ActualCheckout);

        var operations = db.StayOperations
            .Where(x => x.StayId == checkInResult.StayId)
            .OrderBy(x => x.StayOperationId)
            .Select(x => x.OperationType)
            .ToList();

        Assert.Equal(new[] { "CheckIn", "CheckOut" }, operations);
    }
}
