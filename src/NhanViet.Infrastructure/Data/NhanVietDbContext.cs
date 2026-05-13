using Microsoft.EntityFrameworkCore;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Entities;

namespace NhanViet.Infrastructure.Data;

public class NhanVietDbContext(DbContextOptions<NhanVietDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.ApplyConfigurationsFromAssembly(typeof(NhanVietDbContext).Assembly);
    }
}
