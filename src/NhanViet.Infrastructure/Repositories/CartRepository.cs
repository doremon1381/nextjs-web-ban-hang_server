using Microsoft.EntityFrameworkCore;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Interfaces;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Infrastructure.Repositories;

public class CartRepository(NhanVietDbContext db) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public Task<Cart?> GetBySessionIdAsync(string sessionId, CancellationToken ct) =>
        db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId, ct);

    public Task AddAsync(Cart cart, CancellationToken ct) =>
        db.Carts.AddAsync(cart, ct).AsTask();

    public void Update(Cart cart) => db.Carts.Update(cart);

    public void Delete(Cart cart) => db.Carts.Remove(cart);
}
