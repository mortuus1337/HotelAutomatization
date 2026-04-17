using System.Data;
using Hotel.Application.Common;
using Hotel.Application.DTOs.Shifts;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hotel.Infrastructure.Services;

public class WorkShiftService : IWorkShiftService
{
    private const string OpenStatus = "Open";
    private const string ClosedStatus = "Closed";
    private const string OperationCheckIn = "CheckIn";
    private const string OperationCheckInByReservation = "CheckInByReservation";
    private const string OperationCheckOut = "CheckOut";

    private readonly HotelDbContext _dbContext;
    private readonly ILogger<WorkShiftService> _logger;

    public WorkShiftService(
        HotelDbContext dbContext,
        ILogger<WorkShiftService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShiftDto> OpenShiftAsync(
        int currentUserId,
        string? comment,
        bool takeoverIfNeeded,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        if (user is null)
            throw new NotFoundException("Пользователь не найден.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var openShifts = await _dbContext.WorkShifts
            .Include(x => x.User)
            .Where(x => x.Status == OpenStatus)
            .OrderBy(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        var ownOpenShift = openShifts
            .Where(x => x.UserId == currentUserId)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefault();

        if (ownOpenShift is not null)
        {
            var duplicateOwnShifts = openShifts
                .Where(x => x.UserId == currentUserId && x.WorkShiftId != ownOpenShift.WorkShiftId)
                .ToList();

            if (duplicateOwnShifts.Count > 0)
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var duplicate in duplicateOwnShifts)
                {
                    duplicate.Status = ClosedStatus;
                    duplicate.EndedAt = now;
                    duplicate.Comment = MergeComment(duplicate.Comment, "Автозакрытие дубля открытой смены.");
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Open shift request is idempotent. Existing shift returned. ShiftId={ShiftId}, UserId={UserId}",
                ownOpenShift.WorkShiftId,
                currentUserId);

            return MapShift(ownOpenShift);
        }

        var foreignOpenShifts = openShifts
            .Where(x => x.UserId != currentUserId)
            .ToList();

        if (foreignOpenShifts.Count > 0 && !takeoverIfNeeded)
        {
            var activeByAnotherUser = foreignOpenShifts
                .OrderByDescending(x => x.StartedAt)
                .First();

            throw new ValidationException(
                $"Сейчас активна смена администратора {activeByAnotherUser.User.FullName} "
                + $"(открыта {activeByAnotherUser.StartedAt:dd.MM.yyyy HH:mm}). "
                + "Сначала закройте её или используйте режим принятия смены.");
        }

        if (foreignOpenShifts.Count > 0 && takeoverIfNeeded)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var foreignShift in foreignOpenShifts)
            {
                foreignShift.Status = ClosedStatus;
                foreignShift.EndedAt = now;
                foreignShift.Comment = MergeComment(
                    foreignShift.Comment,
                    $"Автозакрытие при принятии смены пользователем ID={currentUserId}.");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shift takeover executed. NewUserId={UserId}, ClosedForeignShifts={ClosedCount}",
                currentUserId,
                foreignOpenShifts.Count);
        }

        var shift = new WorkShift
        {
            UserId = currentUserId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = OpenStatus,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        };

        _dbContext.WorkShifts.Add(shift);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Shift opened. ShiftId={ShiftId}, UserId={UserId}, Takeover={TakeoverIfNeeded}",
            shift.WorkShiftId,
            currentUserId,
            takeoverIfNeeded);

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
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var ownOpenShifts = await _dbContext.WorkShifts
            .Include(x => x.User)
            .Where(x => x.UserId == currentUserId && x.Status == OpenStatus)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        if (ownOpenShifts.Count == 0)
        {
            var activeForeignShift = await _dbContext.WorkShifts
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.UserId != currentUserId && x.Status == OpenStatus)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeForeignShift is not null)
            {
                throw new ValidationException(
                    $"У вас нет открытой смены. Сейчас активна смена администратора "
                    + $"{activeForeignShift.User.FullName}.");
            }

            throw new ValidationException("У пользователя нет открытой смены.");
        }

        var targetShift = ownOpenShifts[0];
        var duplicates = ownOpenShifts.Skip(1).ToList();
        var nowUtc = DateTimeOffset.UtcNow;

        targetShift.EndedAt = nowUtc;
        targetShift.Status = ClosedStatus;

        if (!string.IsNullOrWhiteSpace(comment))
            targetShift.Comment = comment.Trim();

        foreach (var duplicate in duplicates)
        {
            duplicate.EndedAt = nowUtc;
            duplicate.Status = ClosedStatus;
            duplicate.Comment = MergeComment(duplicate.Comment, "Автозакрытие дубля при закрытии смены.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Shift closed. ShiftId={ShiftId}, UserId={UserId}, DuplicatesClosed={DuplicatesClosed}",
            targetShift.WorkShiftId,
            currentUserId,
            duplicates.Count);

        return MapShift(targetShift);
    }

    public async Task<ShiftDto?> GetCurrentShiftAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var currentShift = await _dbContext.WorkShifts
            .Include(x => x.User)
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && x.Status == OpenStatus)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return currentShift is null ? null : MapShift(currentShift);
    }

    public async Task<ShiftDto?> GetActiveShiftAsync(CancellationToken cancellationToken = default)
    {
        var currentShift = await _dbContext.WorkShifts
            .Include(x => x.User)
            .AsNoTracking()
            .Where(x => x.Status == OpenStatus)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return currentShift is null ? null : MapShift(currentShift);
    }

    public async Task<ShiftReportDto> GetShiftReportAsync(
        DateTime? from,
        DateTime? to,
        int? userId,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = from.HasValue
            ? DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc)
            : (DateTime?)null;

        var toUtc = to.HasValue
            ? DateTime.SpecifyKind(to.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : (DateTime?)null;

        var shiftsQuery = _dbContext.WorkShifts
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        if (userId.HasValue)
            shiftsQuery = shiftsQuery.Where(x => x.UserId == userId.Value);

        if (fromUtc.HasValue)
            shiftsQuery = shiftsQuery.Where(x => (x.EndedAt ?? DateTimeOffset.UtcNow) >= fromUtc.Value);

        if (toUtc.HasValue)
            shiftsQuery = shiftsQuery.Where(x => x.StartedAt <= toUtc.Value);

        var shifts = await shiftsQuery
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        if (shifts.Count == 0)
        {
            return new ShiftReportDto
            {
                From = fromUtc,
                To = toUtc,
                UserId = userId
            };
        }

        var userIds = shifts.Select(x => x.UserId).Distinct().ToList();
        var minStartedAt = shifts.Min(x => x.StartedAt);
        var maxEndedAt = shifts.Max(x => x.EndedAt ?? DateTimeOffset.UtcNow);

        var operations = await _dbContext.StayOperations
            .AsNoTracking()
            .Include(x => x.Stay)
                .ThenInclude(x => x.Room)
            .Where(x =>
                userIds.Contains(x.UserId)
                && x.OccurredAt >= minStartedAt
                && x.OccurredAt <= maxEndedAt)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var items = new List<ShiftReportItemDto>(shifts.Count);

        foreach (var shift in shifts)
        {
            var shiftEnd = shift.EndedAt ?? nowUtc;
            var shiftOperations = operations
                .Where(x =>
                    x.UserId == shift.UserId
                    && x.OccurredAt >= shift.StartedAt
                    && x.OccurredAt <= shiftEnd)
                .OrderByDescending(x => x.OccurredAt)
                .ToList();

            var checkInCount = shiftOperations.Count(x =>
                x.OperationType == OperationCheckIn
                || x.OperationType == OperationCheckInByReservation);

            var checkOutCount = shiftOperations.Count(x => x.OperationType == OperationCheckOut);
            var durationMinutes = Math.Max(0, (shiftEnd - shift.StartedAt).TotalMinutes);

            items.Add(new ShiftReportItemDto
            {
                WorkShiftId = shift.WorkShiftId,
                UserId = shift.UserId,
                UserName = shift.User.FullName,
                StartedAt = shift.StartedAt,
                EndedAt = shift.EndedAt,
                Status = shift.Status,
                Comment = shift.Comment,
                DurationMinutes = durationMinutes,
                ActionsCount = shiftOperations.Count,
                CheckInCount = checkInCount,
                CheckOutCount = checkOutCount,
                LastActionAt = shiftOperations.FirstOrDefault()?.OccurredAt,
                Actions = shiftOperations.Select(x => new ShiftActionDto
                {
                    StayOperationId = x.StayOperationId,
                    StayId = x.StayId,
                    RoomNumber = x.Stay.Room.RoomNumber,
                    OperationType = x.OperationType,
                    OccurredAt = x.OccurredAt,
                    Comment = x.Comment
                }).ToList()
            });
        }

        return new ShiftReportDto
        {
            From = fromUtc,
            To = toUtc,
            UserId = userId,
            TotalShifts = items.Count,
            OpenShifts = items.Count(x => x.Status == OpenStatus),
            ClosedShifts = items.Count(x => x.Status == ClosedStatus),
            TotalDurationMinutes = items.Sum(x => x.DurationMinutes),
            TotalActionsCount = items.Sum(x => x.ActionsCount),
            TotalCheckInCount = items.Sum(x => x.CheckInCount),
            TotalCheckOutCount = items.Sum(x => x.CheckOutCount),
            Items = items
        };
    }

    private static ShiftDto MapShift(WorkShift shift)
    {
        return new ShiftDto
        {
            WorkShiftId = shift.WorkShiftId,
            UserId = shift.UserId,
            UserName = shift.User.FullName,
            StartedAt = shift.StartedAt,
            EndedAt = shift.EndedAt,
            Status = shift.Status,
            Comment = shift.Comment
        };
    }

    private static string? MergeComment(string? existing, string addition)
    {
        var additionValue = addition.Trim();
        if (string.IsNullOrWhiteSpace(existing))
            return additionValue;

        return $"{existing.Trim()} | {additionValue}";
    }
}
