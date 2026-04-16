using System;
using System.Collections.Generic;
using System.Text;

using Hotel.Application.DTOs.Rooms;

namespace Hotel.Application.Interfaces;

public interface IRoomTypeService
{
    Task<List<RoomTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto request, CancellationToken cancellationToken = default);
}
