using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhanViet.Domain.Entities;

namespace NhanViet.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Price).HasPrecision(18, 0);
        builder.Property(v => v.OldPrice).HasPrecision(18, 0);
        builder.Ignore(v => v.IsOnSale);
        builder.Ignore(v => v.DiscountPercent);
        builder.Ignore(v => v.InStock);
        builder.UseXminAsConcurrencyToken();
    }
}
