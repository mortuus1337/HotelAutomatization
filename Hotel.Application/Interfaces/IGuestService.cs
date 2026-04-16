using System;
using System.Collections.Generic;
using System.Text;
using Hotel.Application.DTOs.Guests;

namespace Hotel.Application.Interfaces;

public interface IGuestService
{
    Task<List<GuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GuestDto> GetByIdAsync(int guestId, CancellationToken cancellationToken = default);
    Task<GuestDto> CreateAsync(CreateGuestDto request, CancellationToken cancellationToken = default);
    Task<GuestDto> UpdateAsync(int guestId, UpdateGuestDto request, CancellationToken cancellationToken = default);
}

