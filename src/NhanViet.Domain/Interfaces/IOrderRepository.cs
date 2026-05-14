using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;

namespace NhanViet.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByCodeAsync(string orderCode, CancellationToken ct = default);

    Task<(List<Order> Items, int TotalCount)> ListByUserAsync(
        Guid userId, OrderStatus? status, int page, int pageSize, CancellationToken ct = default);

    Task<(List<Order> Items, int TotalCount)> ListAllAsync(
        AdminOrderFilter filter, CancellationToken ct = default);

    Task<OrderDashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);

    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
}

public record AdminOrderFilter(
    OrderStatus? Status = null,
    PaymentStatus? PaymentStatus = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 20
);

public record OrderDashboardStats(
    int OrdersToday,
    decimal Revenue7d,
    int PendingCount,
    int UnpaidCount
);
