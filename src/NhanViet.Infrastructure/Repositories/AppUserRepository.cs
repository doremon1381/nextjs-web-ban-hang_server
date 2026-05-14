using Microsoft.EntityFrameworkCore;
using NhanViet.Application.Auth.Queries;
using NhanViet.Domain.Entities;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Infrastructure.Repositories;

public class AppUserRepository(NhanVietDbContext db) : IAppUserRepository
{
    public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AppUsers.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct) =>
        db.AppUsers.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<(List<AppUser> Items, int TotalCount)> ListAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var q = db.AppUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            q = q.Where(u =>
                EF.Functions.ILike(u.FullName, s) ||
                EF.Functions.ILike(u.Email, s) ||
                (u.Phone != null && EF.Functions.ILike(u.Phone, s)));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task AddAsync(AppUser user, CancellationToken ct) =>
        db.AppUsers.AddAsync(user, ct).AsTask();

    public void Update(AppUser user) => db.AppUsers.Update(user);

    public void Delete(AppUser user) => db.AppUsers.Remove(user);
}
