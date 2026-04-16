using Hotel.Application.Common;
using Hotel.Application.DTOs.Shifts;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class WorkShiftService : IWorkShiftService
{
    private const string OpenStatus = "Open";
    private const string ClosedStatus = "Closed";

    private readonly HotelDbContext _dbContext;

    public WorkShiftService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShiftDto> OpenShiftAsync(int currentUserId, string? comment, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        if (user is null)
            throw new NotFoundException("Пользователь не найден.");

        var existingOpenShift = await _dbContext.WorkShifts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.Status == OpenStatus, cancellationToken);

        if (existingOpenShift is not null)
            throw new ValidationException("У пользователя уже есть открытая смена.");

        var shift = new WorkShift
        {
            UserId = currentUserId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = OpenStatus,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        };

        _dbContext.WorkShifts.Add(shift);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ShiftDto
        {
            WorkShiftId = shift.WorkShiftId,
            UserId = shift.UserId,
            UserName = user.FullName,
            StartedAt = shift.StartedAt,
            EndedAt = shift.EndedAt,
            Status = shift.Status,
            Comment = shift.Comment
        };
    }

    public async Task<ShiftDto> CloseShiftAsync(int currentUserId, string? comment, CancellationToken cancellationToken = default)
    {
        var currentShift = await _dbContext.WorkShifts
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.Status == OpenStatus, cancellationToken);

        if (currentShift is null)
            throw new ValidationException("У пользователя нет открытой смены.");

        currentShift.EndedAt = DateTimeOffset.UtcNow;
        currentShift.Status = ClosedStatus;

        if (!string.IsNullOrWhiteSpace(comment))
            currentShift.Comment = comment.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ShiftDto
        {
            WorkShiftId = currentShift.WorkShiftId,
            UserId = currentShift.UserId,
            UserName = currentShift.User.FullName,
            StartedAt = currentShift.StartedAt,
            EndedAt = currentShift.EndedAt,
            Status = currentShift.Status,
            Comment = currentShift.Comment
        };
    }

    public async Task<ShiftDto?> GetCurrentShiftAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var currentShift = await _dbContext.WorkShifts
            .Include(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.Status == OpenStatus, cancellationToken);

        if (currentShift is null)
            return null;

        return new ShiftDto
        {
            WorkShiftId = currentShift.WorkShiftId,
            UserId = currentShift.UserId,
            UserName = currentShift.User.FullName,
            StartedAt = currentShift.StartedAt,
            EndedAt = currentShift.EndedAt,
            Status = currentShift.Status,
            Comment = currentShift.Comment
        };
    }
}
