using Hotel.Application.DTOs.Documents;

namespace Hotel.Application.Interfaces;

public interface IDocumentService
{
    Task<GeneratedDocumentDto> GenerateServiceContractPdfAsync(
        int reservationId,
        int? generatedByUserId,
        CancellationToken cancellationToken = default);

    Task<GeneratedDocumentDto> GenerateCheckoutActPdfAsync(
        int stayId,
        int? generatedByUserId,
        CancellationToken cancellationToken = default);

    Task<GeneratedDocumentDto> GenerateInvoicePdfAsync(
        int reservationId,
        int? generatedByUserId,
        CancellationToken cancellationToken = default);

    Task<List<GeneratedDocumentHistoryItemDto>> GetHistoryAsync(
        string? documentType,
        DateTime? from,
        DateTime? to,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
