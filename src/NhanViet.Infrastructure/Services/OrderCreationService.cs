using Microsoft.EntityFrameworkCore;
using NhanViet.Application.Orders.Commands;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;
using NhanViet.Infrastructure.Data;
using NhanViet.Application.Common.Exceptions;

namespace NhanViet.Infrastructure.Services;

public class OrderCreationService(
    NhanVietDbContext db,
    IOrderRepository orders
) : IOrderCreationService
{
    public async Task<Order> CreateOrderAtomicallyAsync(
        Cart cart,
        string customerName, string customerPhone, string? customerEmail,
        string shippingAddress, PaymentMethod paymentMethod, string? note,
        decimal shippingFee, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var variantIds = cart.Items.Select(i => i.VariantId).ToList();
        var variants = await db.ProductVariants
            .FromSqlInterpolated($@"SELECT * FROM ""ProductVariants"" WHERE ""Id"" = ANY({variantIds}) FOR UPDATE")
            .ToDictionaryAsync(v => v.Id, ct);

        foreach (var item in cart.Items)
        {
            if (!variants.TryGetValue(item.VariantId, out var variant))
                throw new NotFoundException(nameof(ProductVariant), item.VariantId);
            variant.DeductStock(item.Quantity);
        }

        Order order = null!;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            order = Order.CreateFromCart(
                cart, customerName, customerPhone, customerEmail,
                shippingAddress, paymentMethod, note, shippingFee);
            try
            {
                await orders.AddAsync(order, ct);
                cart.Clear();
                await db.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateException ex) when (IsOrderCodeConflict(ex) && attempt < 4)
            {
                db.ChangeTracker.Clear();
            }
        }

        await tx.CommitAsync(ct);
        return order;
    }

    private static bool IsOrderCodeConflict(DbUpdateException ex) =>
        ex.InnerException is Npgsql.PostgresException pg
        && pg.SqlState == "23505"
        && pg.ConstraintName?.Contains("OrderCode", StringComparison.OrdinalIgnoreCase) == true;
}

public class OrderCancellationService(
    NhanVietDbContext db
) : IOrderCancellationService
{
    public async Task CancelOrderAtomicallyAsync(Order order, string? reason, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var variantIds = order.Items.Select(i => i.VariantId).ToList();
        var variants = await db.ProductVariants
            .FromSqlInterpolated($@"SELECT * FROM ""ProductVariants"" WHERE ""Id"" = ANY({variantIds}) FOR UPDATE")
            .ToDictionaryAsync(v => v.Id, ct);

        foreach (var item in order.Items)
            if (variants.TryGetValue(item.VariantId, out var v))
                v.RestoreStock(item.Quantity);

        order.Cancel(reason);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
