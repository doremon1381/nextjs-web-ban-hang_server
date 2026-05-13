using NhanViet.Domain.Entities;

namespace NhanViet.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Cart?> GetBySessionIdAsync(string sessionId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    void Update(Cart cart);
    void Delete(Cart cart);
}
