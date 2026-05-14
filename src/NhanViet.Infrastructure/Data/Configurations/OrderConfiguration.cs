using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhanViet.Domain.Entities;

namespace NhanViet.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => o.OrderCode).IsUnique();
        builder.HasIndex(o => new { o.UserId, o.Status });
        builder.HasIndex(o => o.CreatedAt).IsDescending();

        builder.Property(o => o.OrderCode).HasMaxLength(20).IsRequired();
        builder.Property(o => o.CustomerName).HasMaxLength(100);
        builder.Property(o => o.CustomerPhone).HasMaxLength(20);
        builder.Property(o => o.CustomerEmail).HasMaxLength(200);
        builder.Property(o => o.ShippingAddress).HasMaxLength(500);
        builder.Property(o => o.Note).HasMaxLength(500);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValueSql("'Unpaid'");
        builder.HasIndex(o => new { o.Status, o.PaymentStatus });
        builder.Property(o => o.Subtotal).HasPrecision(18, 0);
        builder.Property(o => o.ShippingFee).HasPrecision(18, 0);
        builder.Property(o => o.Total).HasPrecision(18, 0);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");

        builder.HasMany(o => o.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_statusHistory");
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductName).HasMaxLength(200);
        builder.Property(i => i.VariantName).HasMaxLength(100);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 0);
        builder.Ignore(i => i.LineTotal);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.Reason).HasMaxLength(500);
    }
}
