using Hotel.Domain.Guests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("guest");

        builder.HasKey(x => x.GuestId);

        builder.Property(x => x.GuestId)
            .HasColumnName("guest_id");

        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(x => x.MiddleName)
            .HasColumnName("middle_name");

        builder.Property(x => x.Phone)
            .HasColumnName("phone");

        builder.Property(x => x.Email)
            .HasColumnName("email");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.HasOne(x => x.GuestIdentity)
            .WithOne(x => x.Guest)
            .HasForeignKey<GuestIdentity>(x => x.GuestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}