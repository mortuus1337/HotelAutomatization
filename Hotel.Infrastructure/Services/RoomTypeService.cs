using Hotel.Application.Common;
using Hotel.Application.DTOs.Rooms;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly HotelDbContext _dbContext;

    public RoomTypeService(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoomTypeDto
            {
                RoomTypeId = (int)x.RoomTypeId,
                Name = x.Name,
                Capacity = x.Capacity,
                BasePrice = x.BasePrice,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Название типа номера обязательно.");

        if (request.Capacity <= 0)
            throw new ValidationException("Вместимость должна быть больше нуля.");

        if (request.BasePrice < 0)
            throw new ValidationException("Базовая цена не может быть отрицательной.");

        var exists = await _dbContext.RoomTypes
            .AnyAsync(x => x.Name == request.Name, cancellationToken);

        if (exists)
            throw new ValidationException("Тип номера с таким названием уже существует.");

        var entity = new RoomType
        {
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            BasePrice = request.BasePrice,
            Description = request.Description
        };

        _dbContext.RoomTypes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RoomTypeDto
        {
            RoomTypeId = (int)entity.RoomTypeId,
            Name = entity.Name,
            Capacity = entity.Capacity,
            BasePrice = entity.BasePrice,
            Description = entity.Description
        };
    }
}