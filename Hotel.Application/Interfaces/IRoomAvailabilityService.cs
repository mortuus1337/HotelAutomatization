using Hotel.Application.DTOs.Reservations;

namespace Hotel.Application.Interfaces;

public interface IRoomAvailabilityService
{
    Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime plannedCheckin,
        DateTime plannedCheckout,
        CancellationToken cancellationToken = default);

    Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime plannedCheckin,
        DateTime plannedCheckout,
        int? excludeReservationId,
        CancellationToken cancellationToken = default);

    Task<List<AvailableRoomDto>> GetAvailableRoomsAsync(
        DateTime plannedCheckin,
        DateTime plannedCheckout,
        CancellationToken cancellationToken = default);
}