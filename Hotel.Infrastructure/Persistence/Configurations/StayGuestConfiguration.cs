using Hotel.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class StayGuestConfiguration : IEntityTypeConfiguration<StayGuest>
{
    public void Configure(EntityTypeBuilder<StayGuest> builder)
    {
        builder.ToTable("stay_guest");

        builder.HasKey(x => new { x.StayId, x.GuestId });

        builder.Property(x => x.StayId)
            .HasColumnName("stay_id");

        builder.Property(x => x.GuestId)
            .HasColumnName("guest_id");

        builder.Property(x => x.IsMain)
            .HasColumnName("is_main");

        builder.HasOne(x => x.Stay)
            .WithMany(x => x.Guests)
            .HasForeignKey(x => x.StayId);

        builder.HasOne(x => x.Guest)
            .WithMany()
            .HasForeignKey(x => x.GuestId);
    }
}