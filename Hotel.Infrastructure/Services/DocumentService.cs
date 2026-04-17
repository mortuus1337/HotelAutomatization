using Hotel.Application.Common;
using Hotel.Application.DTOs.Documents;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Hotel.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private const string DocumentTypeServiceContract = "ServiceContract";
    private const string DocumentTypeCheckoutAct = "CheckoutAct";
    private const string DocumentTypeInvoice = "Invoice";

    private readonly HotelDbContext _dbContext;
    private readonly ILogger<DocumentService> _logger;
    private static bool _questLicenseConfigured;

    public DocumentService(HotelDbContext dbContext, ILogger<DocumentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        EnsureQuestPdfLicense();
    }

    public async Task<GeneratedDocumentDto> GenerateServiceContractPdfAsync(
        int reservationId,
        int? generatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.ReservationRooms)
                .ThenInclude(x => x.Room)
            .Include(x => x.MealPlan)
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронь не найдена.");

        var fileName = $"service-contract-{reservationId}.pdf";
        var documentNumber = $"HC-{reservation.ReservationId:D6}";
        var roomNumbers = reservation.ReservationRooms.Select(x => x.Room.RoomNumber).OrderBy(x => x).ToList();
        var roomText = roomNumbers.Count > 0 ? string.Join(", ", roomNumbers) : "—";
        var nights = Math.Max(1, reservation.PlannedCheckout.DayNumber - reservation.PlannedCheckin.DayNumber);

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("ДОГОВОР НА ОКАЗАНИЕ ГОСТИНИЧНЫХ УСЛУГ").Bold().FontSize(16);
                    column.Item().Text($"№ {documentNumber} от {DateTime.Today:dd.MM.yyyy}");
                    column.Item().PaddingTop(6).Text("Исполнитель: ООО \"Hotel System\"");
                    column.Item().Text($"Заказчик: {reservation.CustomerName}");
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text($"1. Период проживания: {reservation.PlannedCheckin:dd.MM.yyyy} — {reservation.PlannedCheckout:dd.MM.yyyy} ({nights} ночей).");
                    column.Item().Text($"2. Номера: {roomText}.");
                    column.Item().Text($"3. Гости: взрослые — {reservation.Adults}, дети — {reservation.Children}.");
                    column.Item().Text($"4. Питание: {reservation.MealPlan?.Name ?? "не выбрано"}.");
                    column.Item().Text($"5. Стоимость услуг: {reservation.TotalPrice ?? 0m:0.00} ₽.");
                    column.Item().Text($"6. Предоплата: {reservation.Prepayment ?? 0m:0.00} ₽.");
                    column.Item().Text($"7. Источник бронирования: {reservation.Source ?? "не указан"}.");
                    column.Item().Text($"8. Дополнительные условия: {reservation.Comment ?? "без комментариев"}.");
                    column.Item().PaddingTop(12).Text("Стороны подтверждают согласие с условиями оказания услуг и правилами проживания гостиницы.");
                });

                page.Footer().AlignBottom().Row(row =>
                {
                    row.RelativeItem().Text("Исполнитель: ____________________");
                    row.RelativeItem().AlignRight().Text("Заказчик: ____________________");
                });
            });
        }).GeneratePdf();

        await TryWriteHistoryAsync(DocumentTypeServiceContract, fileName, "Reservation", reservationId, generatedByUserId, cancellationToken);

        return new GeneratedDocumentDto
        {
            FileName = fileName,
            Content = bytes
        };
    }

    public async Task<GeneratedDocumentDto> GenerateCheckoutActPdfAsync(
        int stayId,
        int? generatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var stay = await _dbContext.Stays
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.StayGuests)
                .ThenInclude(x => x.Guest)
            .FirstOrDefaultAsync(x => x.StayId == stayId, cancellationToken);

        if (stay is null)
            throw new NotFoundException("Проживание не найдено.");

        var guestNames = stay.StayGuests
            .Select(x => string.Join(" ", new[] { x.Guest.LastName, x.Guest.FirstName, x.Guest.MiddleName }
                .Where(v => !string.IsNullOrWhiteSpace(v))))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var guestsText = guestNames.Count > 0 ? string.Join(", ", guestNames) : "Гости не указаны";
        var actualCheckout = stay.ActualCheckout ?? DateTimeOffset.UtcNow;
        var documentNumber = $"CA-{stay.StayId:D6}";
        var fileName = $"checkout-act-{stayId}.pdf";

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("АКТ О ВЫСЕЛЕНИИ").Bold().FontSize(16);
                    column.Item().Text($"№ {documentNumber} от {actualCheckout:dd.MM.yyyy}");
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text($"Номер: {stay.Room.RoomNumber}");
                    column.Item().Text($"Гости: {guestsText}");
                    column.Item().Text($"Заезд: {stay.PlannedCheckin:dd.MM.yyyy}");
                    column.Item().Text($"Плановый выезд: {stay.PlannedCheckout:dd.MM.yyyy}");
                    column.Item().Text($"Фактический выезд: {actualCheckout:dd.MM.yyyy HH:mm}");
                    column.Item().Text($"Статус проживания: {stay.Status}");
                    column.Item().Text($"Комментарий: {stay.Comment ?? "без комментариев"}");
                    column.Item().PaddingTop(12).Text("Претензий по проживанию и взаиморасчетам на момент выселения не заявлено.");
                });

                page.Footer().AlignBottom().Row(row =>
                {
                    row.RelativeItem().Text("Администратор: ____________________");
                    row.RelativeItem().AlignRight().Text("Гость: ____________________");
                });
            });
        }).GeneratePdf();

        await TryWriteHistoryAsync(DocumentTypeCheckoutAct, fileName, "Stay", stayId, generatedByUserId, cancellationToken);

        return new GeneratedDocumentDto
        {
            FileName = fileName,
            Content = bytes
        };
    }

    public async Task<GeneratedDocumentDto> GenerateInvoicePdfAsync(
        int reservationId,
        int? generatedByUserId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Reservations
            .AsNoTracking()
            .Include(x => x.ReservationRooms)
                .ThenInclude(x => x.Room)
                    .ThenInclude(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Бронь не найдена.");

        var invoiceNumber = $"INV-{reservation.ReservationId:D6}";
        var nights = Math.Max(1, reservation.PlannedCheckout.DayNumber - reservation.PlannedCheckin.DayNumber);
        var totalAmount = reservation.TotalPrice ?? 0m;
        var prepayment = reservation.Prepayment ?? 0m;
        var toPay = Math.Max(0m, totalAmount - prepayment);
        var fileName = $"invoice-{reservationId}.pdf";

        var roomLines = reservation.ReservationRooms
            .Select(x => new
            {
                RoomNumber = x.Room.RoomNumber,
                RoomType = x.Room.RoomType.Name,
                PricePerNight = x.PricePerNight,
                Amount = x.PricePerNight * nights
            })
            .OrderBy(x => x.RoomNumber)
            .ToList();

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("СЧЕТ НА ОПЛАТУ").Bold().FontSize(16);
                    column.Item().Text($"№ {invoiceNumber} от {DateTime.Today:dd.MM.yyyy}");
                    column.Item().Text($"Плательщик: {reservation.CustomerName}");
                    column.Item().Text($"Период: {reservation.PlannedCheckin:dd.MM.yyyy} — {reservation.PlannedCheckout:dd.MM.yyyy} ({nights} ночей)");
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1.4f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Номер");
                            header.Cell().Element(CellStyle).Text("Тип");
                            header.Cell().Element(CellStyle).AlignRight().Text("Цена/ночь");
                            header.Cell().Element(CellStyle).AlignRight().Text("Сумма");
                        });

                        foreach (var line in roomLines)
                        {
                            table.Cell().Element(CellStyle).Text(line.RoomNumber);
                            table.Cell().Element(CellStyle).Text(line.RoomType);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{line.PricePerNight:0.00} ₽");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{line.Amount:0.00} ₽");
                        }
                    });

                    column.Item().AlignRight().Column(summary =>
                    {
                        summary.Item().Text($"Итого: {totalAmount:0.00} ₽");
                        summary.Item().Text($"Предоплата: {prepayment:0.00} ₽");
                        summary.Item().Text($"К доплате: {toPay:0.00} ₽").Bold();
                    });
                });

                page.Footer().AlignBottom().Text("Оплата означает согласие с условиями оказания гостиничных услуг.");
            });
        }).GeneratePdf();

        await TryWriteHistoryAsync(DocumentTypeInvoice, fileName, "Reservation", reservationId, generatedByUserId, cancellationToken);

        return new GeneratedDocumentDto
        {
            FileName = fileName,
            Content = bytes
        };
    }

    public async Task<List<GeneratedDocumentHistoryItemDto>> GetHistoryAsync(
        string? documentType,
        DateTime? from,
        DateTime? to,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var safeLimit = Math.Clamp(limit, 1, 500);

            var query = _dbContext.GeneratedDocuments
                .AsNoTracking()
                .Include(x => x.GeneratedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(documentType))
                query = query.Where(x => x.DocumentType == documentType);

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.GeneratedAt >= fromUtc);
            }

            if (to.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(to.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(x => x.GeneratedAt <= toUtc);
            }

            return await query
                .OrderByDescending(x => x.GeneratedAt)
                .Take(safeLimit)
                .Select(x => new GeneratedDocumentHistoryItemDto
                {
                    GeneratedDocumentId = x.GeneratedDocumentId,
                    DocumentType = x.DocumentType,
                    FileName = x.FileName,
                    EntityType = x.EntityType,
                    EntityId = x.EntityId,
                    GeneratedAt = x.GeneratedAt,
                    GeneratedByUserId = x.GeneratedByUserId,
                    GeneratedByUserName = x.GeneratedByUser != null ? x.GeneratedByUser.FullName : "Unknown"
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load generated documents history. Returning empty list.");
            return new List<GeneratedDocumentHistoryItemDto>();
        }
    }

    private async Task TryWriteHistoryAsync(
        string documentType,
        string fileName,
        string entityType,
        int entityId,
        int? generatedByUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = new GeneratedDocument
            {
                DocumentType = documentType,
                FileName = fileName,
                EntityType = entityType,
                EntityId = entityId,
                GeneratedAt = DateTimeOffset.UtcNow,
                GeneratedByUserId = generatedByUserId
            };

            _dbContext.GeneratedDocuments.Add(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to save generated document history. DocumentType={DocumentType}, EntityType={EntityType}, EntityId={EntityId}",
                documentType,
                entityType,
                entityId);
        }
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(4);
    }

    private static void EnsureQuestPdfLicense()
    {
        if (_questLicenseConfigured)
            return;

        QuestPDF.Settings.License = LicenseType.Community;
        _questLicenseConfigured = true;
    }
}
