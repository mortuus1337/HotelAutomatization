using Hotel.Application.DTOs.Dashboard;

namespace Hotel.Application.Interfaces;

public interface IDashboardService
{
    Task<OwnerDashboardDto> GetOwnerDashboardAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}