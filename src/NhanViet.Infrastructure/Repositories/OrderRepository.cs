using Microsoft.EntityFrameworkCore;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Infrastructure.Repositories;

public class OrderRepository(NhanVietDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByCodeAsync(string orderCode, CancellationToken ct) =>
        db.Orders.Include(o => o.Items).Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct);

    public async Task<(List<Order> Items, int TotalCount)> ListByUserAsync(
        Guid userId, OrderStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<Order> Items, int TotalCount)> ListAllAsync(
        AdminOrderFilter f, CancellationToken ct)
    {
        var q = db.Orders.Include(o => o.Items).AsQueryable();

        if (f.Status.HasValue)        q = q.Where(o => o.Status == f.Status.Value);
        if (f.PaymentStatus.HasValue) q = q.Where(o => o.PaymentStatus == f.PaymentStatus.Value);

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var s = $"%{f.Search.Trim()}%";
            q = q.Where(o =>
                EF.Functions.ILike(o.OrderCode, s) ||
                EF.Functions.ILike(o.CustomerName, s) ||
                EF.Functions.ILike(o.CustomerPhone, s));
        }

        if (f.From.HasValue)
        {
            var fromUtc = f.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(o => o.CreatedAt >= fromUtc);
        }
        if (f.To.HasValue)
        {
            var toUtc = f.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(o => o.CreatedAt < toUtc);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<OrderDashboardStats> GetDashboardStatsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var start7d = startToday.AddDays(-6);

        var ordersToday = await db.Orders.CountAsync(o => o.CreatedAt >= startToday, ct);

        var revenue7d = await db.Orders
            .Where(o => o.CreatedAt >= start7d && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => (decimal?)o.Total, ct) ?? 0m;

        var pending = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
        var unpaid  = await db.Orders.CountAsync(o =>
            o.PaymentStatus == PaymentStatus.Unpaid &&
            o.Status != OrderStatus.Cancelled, ct);

        return new OrderDashboardStats(ordersToday, revenue7d, pending, unpaid);
    }

    public Task AddAsync(Order order, CancellationToken ct) =>
        db.Orders.AddAsync(order, ct).AsTask();

    public void Update(Order order) => db.Orders.Update(order);
}
