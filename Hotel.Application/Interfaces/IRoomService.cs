using System;
using System.Collections.Generic;
using System.Text;

using Hotel.Application.DTOs.Rooms;

namespace Hotel.Application.Interfaces;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomDto request, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateAsync(int roomId, UpdateRoomDto request, CancellationToken cancellationToken = default);
}
