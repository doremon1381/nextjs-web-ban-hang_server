using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhanViet.Domain.Entities;

namespace NhanViet.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.UserId).IsUnique().HasFilter(@"""UserId"" IS NOT NULL");
        builder.HasIndex(c => c.SessionId).IsUnique().HasFilter(@"""SessionId"" IS NOT NULL");

        builder.Ignore(c => c.Subtotal);
        builder.Ignore(c => c.TotalCount);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductName).HasMaxLength(200);
        builder.Property(i => i.VariantName).HasMaxLength(100);
        builder.Property(i => i.ProductImage).HasMaxLength(500);
        builder.Property(i => i.Unit).HasMaxLength(50);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 0);
        builder.Ignore(i => i.LineTotal);
    }
}
