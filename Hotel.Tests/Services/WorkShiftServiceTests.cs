using Hotel.Domain.Entities;
using Hotel.Infrastructure.Services;
using Hotel.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hotel.Tests.Services;

public class WorkShiftServiceTests
{
    [Fact]
    public async Task OpenAndCloseShift_ForCurrentUser_WorksCorrectly()
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
        await db.SaveChangesAsync();

        var service = new WorkShiftService(db, NullLogger<WorkShiftService>.Instance);

        var opened = await service.OpenShiftAsync(1, "Начало смены", takeoverIfNeeded: false);
        Assert.Equal("Open", opened.Status);
        Assert.Equal(1, opened.UserId);
        Assert.NotEqual(0, opened.WorkShiftId);

        var closed = await service.CloseShiftAsync(1, "Конец смены");
        Assert.Equal("Closed", closed.Status);
        Assert.NotNull(closed.EndedAt);

        var shiftInDb = db.WorkShifts.Single();
        Assert.Equal("Closed", shiftInDb.Status);
    }
}
