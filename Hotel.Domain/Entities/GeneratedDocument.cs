namespace Hotel.Domain.Entities;

public class GeneratedDocument
{
    public int GeneratedDocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public int? GeneratedByUserId { get; set; }

    public AppUser? GeneratedByUser { get; set; }
}
