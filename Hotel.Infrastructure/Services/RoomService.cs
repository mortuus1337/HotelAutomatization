using Hotel.Application.Common;
using Hotel.Application.DTOs.Rooms;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly HotelDbContext _dbContext;

    public RoomService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .OrderBy(x => x.RoomNumber)
            .Select(x => new RoomDto
            {
                RoomId = (int)x.RoomId,
                RoomNumber = x.RoomNumber,
                RoomTypeId = (int)x.RoomTypeId,
                RoomTypeName = x.RoomType.Name,
                Floor = (int)x.Floor,
                IsActive = x.IsActive,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomDto> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        var room = await _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.RoomId == roomId, cancellationToken);

        if (room is null)
            throw new NotFoundException("Номер не найден.");

        return new RoomDto
        {
            RoomId = (int)room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomTypeId = (int)room.RoomTypeId,
            RoomTypeName = room.RoomType.Name,
            Floor = (int)room.Floor,
            IsActive = room.IsActive,
            Notes = room.Notes
        };
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RoomNumber))
            throw new ValidationException("Номер комнаты обязателен.");

        if (request.RoomTypeId <= 0)
            throw new ValidationException("Не выбран тип номера.");

        var roomTypeExists = await _dbContext.RoomTypes
            .AnyAsync(x => x.RoomTypeId == request.RoomTypeId, cancellationToken);

        if (!roomTypeExists)
            throw new ValidationException("Указанный тип номера не найден.");

        var roomNumberExists = await _dbContext.Rooms
            .AnyAsync(x => x.RoomNumber == request.RoomNumber, cancellationToken);

        if (roomNumberExists)
            throw new ValidationException("Номер с таким RoomNumber уже существует.");

        var entity = new Room
        {
            RoomNumber = request.RoomNumber.Trim(),
            RoomTypeId = request.RoomTypeId,
            Floor = request.Floor,
            IsActive = request.IsActive,
            Notes = request.Notes
        };

        _dbContext.Rooms.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var room = await _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .FirstAsync(x => x.RoomId == entity.RoomId, cancellationToken);

        return new RoomDto
        {
            RoomId = (int)room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomTypeId = (int)room.RoomTypeId,
            RoomTypeName = room.RoomType.Name,
            Floor = (int)room.Floor,
            IsActive = room.IsActive,
            Notes = room.Notes
        };
    }

    public async Task<RoomDto> UpdateAsync(int roomId, UpdateRoomDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RoomNumber))
            throw new ValidationException("Номер комнаты обязателен.");

        if (request.RoomTypeId <= 0)
            throw new ValidationException("Не выбран тип номера.");

        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(x => x.RoomId == roomId, cancellationToken);

        if (room is null)
            throw new NotFoundException("Номер не найден.");

        var roomTypeExists = await _dbContext.RoomTypes
            .AnyAsync(x => x.RoomTypeId == request.RoomTypeId, cancellationToken);

        if (!roomTypeExists)
            throw new ValidationException("Указанный тип номера не найден.");

        var duplicateRoomNumberExists = await _dbContext.Rooms
            .AnyAsync(x => x.RoomId != roomId && x.RoomNumber == request.RoomNumber, cancellationToken);

        if (duplicateRoomNumberExists)
            throw new ValidationException("Номер с таким RoomNumber уже существует.");

        room.RoomNumber = request.RoomNumber.Trim();
        room.RoomTypeId = request.RoomTypeId;
        room.Floor = request.Floor;
        room.IsActive = request.IsActive;
        room.Notes = request.Notes;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedRoom = await _dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .FirstAsync(x => x.RoomId == roomId, cancellationToken);

        return new RoomDto
        {
            RoomId = (int)updatedRoom.RoomId,
            RoomNumber = updatedRoom.RoomNumber,
            RoomTypeId = (int)updatedRoom.RoomTypeId,
            RoomTypeName = updatedRoom.RoomType.Name,
            Floor = (int)updatedRoom.Floor,
            IsActive = updatedRoom.IsActive,
            Notes = updatedRoom.Notes
        };
    }
}