using Hotel.Application.DTOs.Stays;

namespace Hotel.Application.Interfaces;

public interface IStayService
{
    Task<List<CurrentStayDto>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<List<StayOperationDto>> GetOperationsAsync(
        DateTime? from,
        DateTime? to,
        int? userId,
        CancellationToken cancellationToken = default);
    Task<StayDto> GetByIdAsync(int stayId, CancellationToken cancellationToken = default);
    Task<StayDto> CheckInAsync(CreateStayDto request, int currentUserId, CancellationToken cancellationToken = default);
    Task<StayDto> CheckInByReservationAsync(CreateStayFromReservationDto request, int currentUserId, CancellationToken cancellationToken = default);
    Task<StayDto> CheckOutAsync(int stayId, CheckOutStayDto request, int currentUserId, CancellationToken cancellationToken = default);
    Task<List<CurrentGuestDto>> GetCurrentGuestsAsync(CancellationToken cancellationToken = default);
}
