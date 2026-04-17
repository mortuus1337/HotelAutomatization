using Hotel.Application.Common;
using Hotel.Application.DTOs.Stays;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hotel.Infrastructure.Services;

public class StayService : IStayService
{
    private const string OperationCheckIn = "CheckIn";
    private const string OperationCheckInByReservation = "CheckInByReservation";
    private const string OperationCheckOut = "CheckOut";

    private readonly HotelDbContext _dbContext;
    private readonly IRoomAvailabilityService _roomAvailabilityService;
    private readonly ILogger<StayService> _logger;

    public StayService(
        HotelDbContext dbContext,
        IRoomAvailabilityService roomAvailabilityService,
        ILogger<StayService> logger)
    {
        _dbContext = dbContext;
        _roomAvailabilityService = roomAvailabilityService;
        _logger = logger;
    }

    public async Task<List<CurrentStayDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stays
            .AsNoTracking()
            .Include(x => x.Room)
            .Where(x => x.Status == StayStatuses.Planned || x.Status == StayStatuses.Active)
            .OrderBy(x => x.Room.RoomNumber)
            .Select(x => new CurrentStayDto
            {
                StayId = x.StayId,
                RoomId = x.RoomId,
                RoomNumber = x.Room.RoomNumber,
                Status = x.Status,
                ActualCheckin = x.ActualCheckin,
                PlannedCheckout = x.PlannedCheckout.ToDateTime(TimeOnly.MinValue),
                Comment = x.Comment
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StayOperationDto>> GetOperationsAsync(
        DateTime? from,
        DateTime? to,
        int? userId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StayOperations
            .AsNoTracking()
            .Include(x => x.Stay)
                .ThenInclude(x => x.Room)
            .Include(x => x.User)
            .AsQueryable();

        if (from.HasValue)
        {
            var fromValue = from.Value.Date;
            var fromUtc = fromValue.Kind == DateTimeKind.Utc
                ? fromValue
                : DateTime.SpecifyKind(fromValue, DateTimeKind.Utc);
            query = query.Where(x => x.OccurredAt >= fromUtc);
        }

        if (to.HasValue)
        {
            var toValue = to.Value.Date.AddDays(1).AddTicks(-1);
            var toUtc = toValue.Kind == DateTimeKind.Utc
                ? toValue
                : DateTime.SpecifyKind(toValue, DateTimeKind.Utc);
            query = query.Where(x => x.OccurredAt <= toUtc);
        }

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new StayOperationDto
            {
                StayOperationId = x.StayOperationId,
                StayId = x.StayId,
                RoomId = x.Stay.RoomId,
                RoomNumber = x.Stay.Room.RoomNumber,
                UserId = x.UserId,
                UserName = x.User.FullName,
                OperationType = x.OperationType,
                OccurredAt = x.OccurredAt,
                Comment = x.Comment
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<StayDto> GetByIdAsync(int stayId, CancellationToken cancellationToken = default)
    {
        var stay = await LoadStay(stayId, cancellationToken);

        if (stay is null)
            throw new NotFoundException("Проживание не найдено.");

        return MapStay(stay);
    }

    public async Task<StayDto> CheckInAsync(CreateStayDto request, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (request.RoomId <= 0)
            throw new ValidationException("Не выбран номер.");

        if (request.GuestIds is null || request.GuestIds.Count == 0)
            throw new ValidationException("Нужно указать хотя бы одного гостя.");

        ValidateDates(request.PlannedCheckin, request.PlannedCheckout);

        var userExists = await _dbContext.AppUsers
            .AnyAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        if (!userExists)
            throw new ValidationException("Текущий пользователь не найден.");

        var roomExists = await _dbContext.Rooms
            .AnyAsync(x => x.RoomId == request.RoomId && x.IsActive, cancellationToken);

        if (!roomExists)
            throw new ValidationException("Номер не найден или неактивен.");

        if (request.MealPlanId.HasValue)
        {
            var mealPlanExists = await _dbContext.MealPlans
                .AnyAsync(x => x.MealPlanId == request.MealPlanId.Value, cancellationToken);

            if (!mealPlanExists)
                throw new ValidationException("Указанный тип питания не найден.");
        }

        var distinctGuestIds = request.GuestIds.Distinct().ToList();

        var guests = await _dbContext.Guests
            .AsNoTracking()
            .Include(x => x.GuestIdentity)
            .Where(x => distinctGuestIds.Contains(x.GuestId))
            .ToListAsync(cancellationToken);

        if (guests.Count != distinctGuestIds.Count)
            throw new ValidationException("Один или несколько гостей не найдены.");

        ValidateGuestsIdentityForCheckIn(guests);

        var isAvailable = await _roomAvailabilityService.IsRoomAvailableAsync(
            request.RoomId,
            request.PlannedCheckin,
            request.PlannedCheckout,
            cancellationToken);

        if (!isAvailable)
            throw new ValidationException("Номер недоступен на выбранные даты.");

        var stay = new Stay
        {
            ReservationId = null,
            RoomId = request.RoomId,
            Status = StayStatuses.Active,
            ActualCheckin = DateTimeOffset.UtcNow,
            ActualCheckout = null,
            PlannedCheckin = DateOnly.FromDateTime(request.PlannedCheckin),
            PlannedCheckout = DateOnly.FromDateTime(request.PlannedCheckout),
            MealPlanId = request.MealPlanId,
            CreatedByUserId = currentUserId,
            Comment = request.Comment
        };

        _dbContext.Stays.Add(stay);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var stayGuests = distinctGuestIds.Select((guestId, index) => new StayGuest
        {
            StayId = stay.StayId,
            GuestId = guestId,
            IsMain = index == 0
        }).ToList();

        _dbContext.StayGuests.AddRange(stayGuests);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AddOperationAsync(
            stay.StayId,
            currentUserId,
            OperationCheckIn,
            request.Comment,
            cancellationToken);

        _logger.LogInformation(
            "Stay check-in completed. StayId={StayId}, RoomId={RoomId}, UserId={UserId}, Guests={GuestsCount}",
            stay.StayId,
            stay.RoomId,
            currentUserId,
            distinctGuestIds.Count);

        return await GetByIdAsync(stay.StayId, cancellationToken);
    }

    public async Task<StayDto> CheckInByReservationAsync(CreateStayFromReservationDto request, int currentUserId, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Reservations
            .Include(x => x.ReservationRooms)
            .FirstOrDefaultAsync(x => x.ReservationId == request.ReservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронь не найдена.");

        if (reservation.Status == ReservationStatuses.Cancelled)
            throw new ValidationException("Нельзя заселить по отменённой брони.");

        if (reservation.Status == ReservationStatuses.CheckedIn)
            throw new ValidationException("По этой брони уже выполнено заселение.");

        var reservedRoom = reservation.ReservationRooms.FirstOrDefault();

        if (reservedRoom is null)
            throw new ValidationException("В брони нет номера для заселения.");

        var isAvailable = await _roomAvailabilityService.IsRoomAvailableAsync(
            reservedRoom.RoomId,
            reservation.PlannedCheckin.ToDateTime(TimeOnly.MinValue),
            reservation.PlannedCheckout.ToDateTime(TimeOnly.MinValue),
            reservation.ReservationId,
            cancellationToken);

        if (!isAvailable)
            throw new ValidationException("Номер по брони уже недоступен.");

        var stay = new Stay
        {
            ReservationId = reservation.ReservationId,
            RoomId = reservedRoom.RoomId,
            Status = StayStatuses.Active,
            ActualCheckin = DateTimeOffset.UtcNow,
            PlannedCheckin = reservation.PlannedCheckin,
            PlannedCheckout = reservation.PlannedCheckout,
            MealPlanId = reservation.MealPlanId,
            CreatedByUserId = currentUserId,
            Comment = request.Comment ?? reservation.Comment
        };

        _dbContext.Stays.Add(stay);
        await _dbContext.SaveChangesAsync(cancellationToken);

        reservation.Status = ReservationStatuses.CheckedIn;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AddOperationAsync(
            stay.StayId,
            currentUserId,
            OperationCheckInByReservation,
            request.Comment ?? reservation.Comment,
            cancellationToken);

        _logger.LogInformation(
            "Stay check-in by reservation completed. StayId={StayId}, ReservationId={ReservationId}, RoomId={RoomId}, UserId={UserId}",
            stay.StayId,
            reservation.ReservationId,
            stay.RoomId,
            currentUserId);

        return await GetByIdAsync(stay.StayId, cancellationToken);
    }

    public async Task<StayDto> CheckOutAsync(
        int stayId,
        CheckOutStayDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var stay = await _dbContext.Stays
            .FirstOrDefaultAsync(x => x.StayId == stayId, cancellationToken);

        if (stay is null)
            throw new NotFoundException("Проживание не найдено.");

        if (stay.Status == StayStatuses.CheckedOut)
            throw new ValidationException("Проживание уже закрыто.");

        stay.Status = StayStatuses.CheckedOut;
        stay.ActualCheckout = request.ActualCheckout ?? DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Comment))
            stay.Comment = request.Comment;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AddOperationAsync(
            stay.StayId,
            currentUserId,
            OperationCheckOut,
            request.Comment,
            cancellationToken);

        _logger.LogInformation(
            "Stay check-out completed. StayId={StayId}, RoomId={RoomId}, UserId={UserId}, ActualCheckout={ActualCheckout}",
            stay.StayId,
            stay.RoomId,
            currentUserId,
            stay.ActualCheckout);

        return await GetByIdAsync(stay.StayId, cancellationToken);
    }

    public async Task<List<CurrentGuestDto>> GetCurrentGuestsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.StayGuests
            .AsNoTracking()
            .Include(x => x.Stay)
                .ThenInclude(x => x.Room)
            .Include(x => x.Guest)
            .Where(x => x.Stay.Status == StayStatuses.Planned || x.Stay.Status == StayStatuses.Active)
            .OrderBy(x => x.Stay.Room.RoomNumber)
            .ThenByDescending(x => x.IsMain)
            .Select(x => new CurrentGuestDto
            {
                StayId = x.StayId,
                RoomId = x.Stay.RoomId,
                RoomNumber = x.Stay.Room.RoomNumber,
                GuestId = x.GuestId,
                LastName = x.Guest.LastName,
                FirstName = x.Guest.FirstName,
                MiddleName = x.Guest.MiddleName,
                Phone = x.Guest.Phone,
                IsMain = x.IsMain
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<Stay?> LoadStay(int stayId, CancellationToken cancellationToken)
    {
        return await _dbContext.Stays
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.MealPlan)
            .Include(x => x.StayGuests)
                .ThenInclude(x => x.Guest)
            .FirstOrDefaultAsync(x => x.StayId == stayId, cancellationToken);
    }

    private static StayDto MapStay(Stay stay)
    {
        return new StayDto
        {
            StayId = stay.StayId,
            ReservationId = stay.ReservationId,
            RoomId = stay.RoomId,
            RoomNumber = stay.Room.RoomNumber,
            Status = stay.Status,
            ActualCheckin = stay.ActualCheckin,
            ActualCheckout = stay.ActualCheckout,
            PlannedCheckin = stay.PlannedCheckin.ToDateTime(TimeOnly.MinValue),
            PlannedCheckout = stay.PlannedCheckout.ToDateTime(TimeOnly.MinValue),
            MealPlanId = stay.MealPlanId,
            MealPlanName = stay.MealPlan?.Name,
            CreatedByUserId = (int)stay.CreatedByUserId,
            Comment = stay.Comment,
            Guests = stay.StayGuests
                .Select(x => new StayGuestDto
                {
                    GuestId = x.GuestId,
                    LastName = x.Guest.LastName,
                    FirstName = x.Guest.FirstName,
                    MiddleName = x.Guest.MiddleName,
                    Phone = x.Guest.Phone,
                    IsMain = x.IsMain
                })
                .ToList()
        };
    }

    private static void ValidateDates(DateTime plannedCheckin, DateTime plannedCheckout)
    {
        if (plannedCheckin == default)
            throw new ValidationException("Дата заезда обязательна.");

        if (plannedCheckout == default)
            throw new ValidationException("Дата выезда обязательна.");

        if (plannedCheckout <= plannedCheckin)
            throw new ValidationException("Дата выезда должна быть позже даты заезда.");
    }

    private static void ValidateGuestsIdentityForCheckIn(IEnumerable<Guest> guests)
    {
        var invalidGuests = new List<string>();

        foreach (var guest in guests)
        {
            var missing = GetMissingIdentityFields(guest.GuestIdentity);
            if (missing.Count == 0)
                continue;

            var guestName = string.Join(
                " ",
                new[] { guest.LastName, guest.FirstName, guest.MiddleName }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

            invalidGuests.Add($"{guestName} (ID={guest.GuestId}): {string.Join(", ", missing)}");
        }

        if (invalidGuests.Count == 0)
            return;

        throw new ValidationException(
            "Для заселения у гостя должны быть заполнены паспортные данные: "
            + string.Join(" | ", invalidGuests));
    }

    private static List<string> GetMissingIdentityFields(GuestIdentity? identity)
    {
        var missing = new List<string>();

        if (identity is null)
        {
            missing.Add("тип документа");
            missing.Add("номер документа");
            missing.Add("кем выдан");
            missing.Add("дата выдачи");
            missing.Add("дата рождения");
            missing.Add("гражданство");
            missing.Add("адрес");
            return missing;
        }

        if (string.IsNullOrWhiteSpace(identity.DocType))
            missing.Add("тип документа");

        if (string.IsNullOrWhiteSpace(identity.DocNumber))
            missing.Add("номер документа");

        if (string.IsNullOrWhiteSpace(identity.IssuedBy))
            missing.Add("кем выдан");

        if (!identity.IssuedDate.HasValue)
            missing.Add("дата выдачи");

        if (!identity.BirthDate.HasValue)
            missing.Add("дата рождения");

        if (string.IsNullOrWhiteSpace(identity.Citizenship))
            missing.Add("гражданство");

        if (string.IsNullOrWhiteSpace(identity.Address))
            missing.Add("адрес");

        return missing;
    }

    private async Task AddOperationAsync(
        int stayId,
        int userId,
        string operationType,
        string? comment,
        CancellationToken cancellationToken)
    {
        var operation = new StayOperation
        {
            StayId = stayId,
            UserId = userId,
            OperationType = operationType,
            OccurredAt = DateTimeOffset.UtcNow,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        };

        _dbContext.StayOperations.Add(operation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
