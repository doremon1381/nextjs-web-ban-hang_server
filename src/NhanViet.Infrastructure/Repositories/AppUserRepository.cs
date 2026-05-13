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

    public Task AddAsync(AppUser user, CancellationToken ct) =>
        db.AppUsers.AddAsync(user, ct).AsTask();

    public void Update(AppUser user) => db.AppUsers.Update(user);

    public void Delete(AppUser user) => db.AppUsers.Remove(user);
}
