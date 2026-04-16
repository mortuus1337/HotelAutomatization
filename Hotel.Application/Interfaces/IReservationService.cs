using Hotel.Application.DTOs.Reservations;

namespace Hotel.Application.Interfaces;

public interface IReservationService
{
    Task<List<ReservationListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ReservationDto> GetByIdAsync(int reservationId, CancellationToken cancellationToken = default);
    Task<ReservationDto> CreateAsync(
        CreateReservationDto request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ReservationDto> ConfirmAsync(int reservationId, CancellationToken cancellationToken = default);
    Task<ReservationDto> CancelAsync(int reservationId, CancellationToken cancellationToken = default);
}