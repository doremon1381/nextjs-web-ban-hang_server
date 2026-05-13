using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhanViet.Domain.Entities;

namespace NhanViet.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("app_users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.Address).HasMaxLength(500);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("Customer");
        builder.Property(u => u.AuthProvider).HasMaxLength(20);

        builder.HasMany(u => u.Orders)
            .WithOne()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Cart)
            .WithOne()
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(100);
        builder.Property(m => m.Phone).HasMaxLength(20);
        builder.Property(m => m.Email).HasMaxLength(200);
        builder.Property(m => m.Subject).HasMaxLength(200);
        builder.Property(m => m.Message).HasMaxLength(2000);
    }
}
