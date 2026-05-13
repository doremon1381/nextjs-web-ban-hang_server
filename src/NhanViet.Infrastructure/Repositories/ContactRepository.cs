using Microsoft.EntityFrameworkCore;
using NhanViet.Application.Contact.Commands;
using NhanViet.Domain.Entities;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Infrastructure.Repositories;

public class ContactRepository(NhanVietDbContext db) : IContactRepository
{
    public Task AddAsync(ContactMessage message, CancellationToken ct) =>
        db.ContactMessages.AddAsync(message, ct).AsTask();

    public async Task<(List<ContactMessage> Items, int TotalCount)> ListAsync(
        bool? isRead, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ContactMessages.AsQueryable();
        if (isRead.HasValue) query = query.Where(m => m.IsRead == isRead.Value);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id, ct);

    public void Update(ContactMessage message) => db.ContactMessages.Update(message);
}
