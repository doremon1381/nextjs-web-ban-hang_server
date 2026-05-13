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
        OrderStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
}
