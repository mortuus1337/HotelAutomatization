using Hotel.Application.DTOs.Shifts;

namespace Hotel.Application.Interfaces;

public interface IWorkShiftService
{
    Task<ShiftDto> OpenShiftAsync(
        int currentUserId,
        string? comment,
        bool takeoverIfNeeded,
        CancellationToken cancellationToken = default);
    Task<ShiftDto> CloseShiftAsync(int currentUserId, string? comment, CancellationToken cancellationToken = default);
    Task<ShiftDto?> GetCurrentShiftAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<ShiftDto?> GetActiveShiftAsync(CancellationToken cancellationToken = default);
    Task<ShiftReportDto> GetShiftReportAsync(
        DateTime? from,
        DateTime? to,
        int? userId,
        CancellationToken cancellationToken = default);
}
