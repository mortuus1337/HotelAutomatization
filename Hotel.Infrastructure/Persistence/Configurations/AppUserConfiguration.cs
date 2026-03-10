using Hotel.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("app_user");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.Login)
            .HasColumnName("login")
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .IsRequired();

        builder.Property(x => x.RoleCode)
            .HasColumnName("role_code")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.HasIndex(x => x.Login)
            .IsUnique();
    }
}