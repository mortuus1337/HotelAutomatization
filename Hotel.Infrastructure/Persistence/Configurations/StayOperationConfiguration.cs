using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class StayOperationConfiguration : IEntityTypeConfiguration<StayOperation>
{
    public void Configure(EntityTypeBuilder<StayOperation> builder)
    {
        builder.ToTable("stay_operation");

        builder.HasKey(x => x.StayOperationId);

        builder.Property(x => x.StayOperationId).HasColumnName("stay_operation_id");
        builder.Property(x => x.StayId).HasColumnName("stay_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.OperationType)
            .HasColumnName("operation_type")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasMaxLength(500);

        builder.HasOne(x => x.Stay)
            .WithMany(x => x.Operations)
            .HasForeignKey(x => x.StayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.StayOperations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
    }
}

