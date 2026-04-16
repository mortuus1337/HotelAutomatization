using Hotel.Application.DTOs.Shifts;

namespace Hotel.Application.Interfaces;

public interface IWorkShiftService
{
    Task<ShiftDto> OpenShiftAsync(int currentUserId, string? comment, CancellationToken cancellationToken = default);
    Task<ShiftDto> CloseShiftAsync(int currentUserId, string? comment, CancellationToken cancellationToken = default);
    Task<ShiftDto?> GetCurrentShiftAsync(int currentUserId, CancellationToken cancellationToken = default);
}
