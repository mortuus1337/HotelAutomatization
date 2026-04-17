using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class GeneratedDocumentConfiguration : IEntityTypeConfiguration<GeneratedDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedDocument> builder)
    {
        builder.ToTable("generated_document");

        builder.HasKey(x => x.GeneratedDocumentId);

        builder.Property(x => x.GeneratedDocumentId).HasColumnName("generated_document_id");
        builder.Property(x => x.DocumentType)
            .HasColumnName("document_type")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.EntityId).HasColumnName("entity_id");
        builder.Property(x => x.GeneratedAt).HasColumnName("generated_at");
        builder.Property(x => x.GeneratedByUserId).HasColumnName("generated_by_user_id");

        builder.HasOne(x => x.GeneratedByUser)
            .WithMany(x => x.GeneratedDocuments)
            .HasForeignKey(x => x.GeneratedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.GeneratedAt);
        builder.HasIndex(x => new { x.DocumentType, x.GeneratedAt });
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
