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
        OrderStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Orders.Include(o => o.Items).AsQueryable();
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public Task AddAsync(Order order, CancellationToken ct) =>
        db.Orders.AddAsync(order, ct).AsTask();

    public void Update(Order order) => db.Orders.Update(order);
}
