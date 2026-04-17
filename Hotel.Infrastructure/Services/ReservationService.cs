using Hotel.Application.Common;
using Hotel.Application.DTOs.Reservations;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hotel.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private readonly HotelDbContext _dbContext;
    private readonly IRoomAvailabilityService _roomAvailabilityService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        HotelDbContext dbContext,
        IRoomAvailabilityService roomAvailabilityService,
        ILogger<ReservationService> logger)
    {
        _dbContext = dbContext;
        _roomAvailabilityService = roomAvailabilityService;
        _logger = logger;
    }

    public async Task<List<ReservationListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReservationListItemDto
            {
                ReservationId = (int)x.ReservationId,
                Status = x.Status,
                CustomerName = x.CustomerName,
                CustomerPhone = x.CustomerPhone,
                PlannedCheckin = x.PlannedCheckin.ToDateTime(TimeOnly.MinValue),
                PlannedCheckout = x.PlannedCheckout.ToDateTime(TimeOnly.MinValue),
                Adults = x.Adults,
                Children = x.Children,
                TotalPrice = (decimal)x.TotalPrice
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ReservationDto> GetByIdAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.MealPlan)
            .Include(x => x.ReservationRooms)
                .ThenInclude(x => x.Room)
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронь не найдена.");

        return MapReservation(reservation);
    }

    public async Task<ReservationDto> CreateAsync(
        CreateReservationDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var userExists = await _dbContext.AppUsers
            .AnyAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        if (!userExists)
            throw new ValidationException("Текущий пользователь не найден.");

        if (request.MealPlanId.HasValue)
        {
            var mealPlanExists = await _dbContext.MealPlans
                .AnyAsync(x => x.MealPlanId == request.MealPlanId.Value, cancellationToken);

            if (!mealPlanExists)
                throw new ValidationException("Указанный тип питания не найден.");
        }

        var distinctRoomIds = request.RoomIds
            .Distinct()
            .ToList();

        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .Where(x => distinctRoomIds.Contains((int)x.RoomId) && x.IsActive)
            .ToListAsync(cancellationToken);

        if (rooms.Count != distinctRoomIds.Count)
            throw new ValidationException("Один или несколько выбранных номеров не найдены.");

        foreach (var roomId in distinctRoomIds)
        {
            var isAvailable = await _roomAvailabilityService.IsRoomAvailableAsync(
                roomId,
                request.PlannedCheckin,
                request.PlannedCheckout,
                cancellationToken);

            if (!isAvailable)
                throw new ValidationException($"Номер с RoomId={roomId} недоступен на выбранные даты.");
        }

        var plannedCheckinDate = DateOnly.FromDateTime(request.PlannedCheckin);
        var plannedCheckoutDate = DateOnly.FromDateTime(request.PlannedCheckout);

        var nights = plannedCheckoutDate.DayNumber - plannedCheckinDate.DayNumber;
        var totalPrice = rooms.Sum(x => x.RoomType.BasePrice * nights);

        var reservation = new Reservation
        {
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserId,
            Status = ReservationStatuses.Created,
            Source = request.Source,
            CustomerName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone,
            Comment = request.Comment,
            PlannedCheckin = plannedCheckinDate,
            PlannedCheckout = plannedCheckoutDate,
            Adults = request.Adults,
            Children = request.Children,
            TotalPrice = totalPrice,
            Prepayment = request.Prepayment,
            MealPlanId = request.MealPlanId
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var reservationRooms = rooms.Select(room => new ReservationRoom
        {
            ReservationId = reservation.ReservationId,
            RoomId = room.RoomId,
            PricePerNight = room.RoomType.BasePrice
        }).ToList();

        _dbContext.ReservationRooms.AddRange(reservationRooms);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdReservation = await _dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.MealPlan)
            .Include(x => x.ReservationRooms)
                .ThenInclude(x => x.Room)
            .FirstAsync(x => x.ReservationId == reservation.ReservationId, cancellationToken);

        _logger.LogInformation(
            "Reservation created. ReservationId={ReservationId}, UserId={UserId}, Rooms={RoomsCount}, TotalPrice={TotalPrice}",
            reservation.ReservationId,
            currentUserId,
            reservationRooms.Count,
            reservation.TotalPrice);

        return MapReservation(createdReservation);
    }

    public async Task<ReservationDto> ConfirmAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Reservations
            .Include(x => x.MealPlan)
            .Include(x => x.ReservationRooms)
                .ThenInclude(x => x.Room)
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронь не найдена.");

        if (reservation.Status == ReservationStatuses.Cancelled)
            throw new ValidationException("Нельзя подтвердить отменённую бронь.");

        reservation.Status = ReservationStatuses.Confirmed;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reservation confirmed. ReservationId={ReservationId}, Status={Status}",
            reservation.ReservationId,
            reservation.Status);

        return MapReservation(reservation);
    }

    public async Task<ReservationDto> CancelAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Reservations
            .Include(x => x.MealPlan)
            .Include(x => x.ReservationRooms)
                .ThenInclude(x => x.Room)
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронь не найдена.");

        if (reservation.Status == ReservationStatuses.CheckedIn)
            throw new ValidationException("Нельзя отменить бронь после заселения.");

        reservation.Status = ReservationStatuses.Cancelled;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reservation canceled. ReservationId={ReservationId}, Status={Status}",
            reservation.ReservationId,
            reservation.Status);

        return MapReservation(reservation);
    }

    private static void ValidateCreateRequest(CreateReservationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ValidationException("Имя заказчика обязательно.");

        if (request.PlannedCheckin == default)
            throw new ValidationException("Дата заезда обязательна.");

        if (request.PlannedCheckout == default)
            throw new ValidationException("Дата выезда обязательна.");

        if (request.PlannedCheckout <= request.PlannedCheckin)
            throw new ValidationException("Дата выезда должна быть позже даты заезда.");

        if (request.RoomIds is null || request.RoomIds.Count == 0)
            throw new ValidationException("Нужно выбрать хотя бы один номер.");

        if (request.Adults < 0)
            throw new ValidationException("Количество взрослых не может быть отрицательным.");

        if (request.Children < 0)
            throw new ValidationException("Количество детей не может быть отрицательным.");

        if (request.Prepayment < 0)
            throw new ValidationException("Предоплата не может быть отрицательной.");
    }

    private static ReservationDto MapReservation(Reservation reservation)
    {
        return new ReservationDto
        {
            ReservationId = (int)reservation.ReservationId,
            CreatedAt = reservation.CreatedAt,
            CreatedByUserId = (int)reservation.CreatedByUserId,
            Status = reservation.Status,
            Source = reservation.Source,
            CustomerName = reservation.CustomerName,
            CustomerPhone = reservation.CustomerPhone,
            Comment = reservation.Comment,
            PlannedCheckin = reservation.PlannedCheckin.ToDateTime(TimeOnly.MinValue),
            PlannedCheckout = reservation.PlannedCheckout.ToDateTime(TimeOnly.MinValue),
            Adults = reservation.Adults,
            Children = reservation.Children,
            TotalPrice = (decimal)reservation.TotalPrice,
            Prepayment = (decimal)reservation.Prepayment,
            MealPlanId = (int?)reservation.MealPlanId,
            MealPlanName = reservation.MealPlan?.Name,
            Rooms = reservation.ReservationRooms
                .Select(x => new ReservationRoomDto
                {
                    ReservationRoomId = (int)x.ReservationRoomId,
                    RoomId = (int)x.RoomId,
                    RoomNumber = x.Room.RoomNumber,
                    PricePerNight = (decimal)x.PricePerNight
                })
                .ToList()
        };
    }
}
