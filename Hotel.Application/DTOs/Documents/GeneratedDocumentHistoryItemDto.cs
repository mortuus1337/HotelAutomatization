namespace Hotel.Application.DTOs.Documents;

public class GeneratedDocumentHistoryItemDto
{
    public int GeneratedDocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public int? GeneratedByUserId { get; set; }
    public string GeneratedByUserName { get; set; } = string.Empty;
}
