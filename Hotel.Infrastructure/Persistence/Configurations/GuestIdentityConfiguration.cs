using Hotel.Domain.Guests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class GuestIdentityConfiguration : IEntityTypeConfiguration<GuestIdentity>
{
    public void Configure(EntityTypeBuilder<GuestIdentity> builder)
    {
        builder.ToTable("guest_identity");

        builder.HasKey(x => x.GuestId);

        builder.Property(x => x.GuestId)
            .HasColumnName("guest_id");

        builder.Property(x => x.DocType)
            .HasColumnName("doc_type")
            .IsRequired();

        builder.Property(x => x.DocNumber)
            .HasColumnName("doc_number")
            .IsRequired();

        builder.Property(x => x.IssuedBy)
            .HasColumnName("issued_by");

        builder.Property(x => x.IssuedDate)
            .HasColumnName("issued_date");

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(x => x.Citizenship)
            .HasColumnName("citizenship");

        builder.Property(x => x.Address)
            .HasColumnName("address");
    }
}