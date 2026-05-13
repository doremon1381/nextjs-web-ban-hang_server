using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhanViet.Domain.Entities;

namespace NhanViet.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.FullDescription).HasMaxLength(2000);
        builder.Property(p => p.Unit).HasMaxLength(50);
        builder.Property(p => p.Image).HasMaxLength(500);
        builder.Property(p => p.Badge).HasMaxLength(50);
        builder.Property(p => p.Origin).HasMaxLength(500);
        builder.Property(p => p.Harvest).HasMaxLength(500);
        builder.Property(p => p.Packaging).HasMaxLength(500);
        builder.Property(p => p.Storage).HasMaxLength(500);
        builder.Property(p => p.Shipping).HasMaxLength(500);
        builder.Property(p => p.Rating).HasPrecision(3, 1);
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Images).HasColumnType("jsonb");

        builder.Ignore(p => p.Price);
        builder.Ignore(p => p.OldPrice);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Variants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_variants");

        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => new { p.Featured, p.IsActive });
    }
}
