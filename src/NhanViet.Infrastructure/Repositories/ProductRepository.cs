using Microsoft.EntityFrameworkCore;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Interfaces;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Infrastructure.Repositories;

public class ProductRepository(NhanVietDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct) =>
        db.Products.Include(p => p.Variants.Where(v => v.IsActive))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct);

    public async Task<(List<Product> Items, int TotalCount)> ListAsync(ProductFilter filter, CancellationToken ct)
    {
        var query = db.Products
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Where(p => p.IsActive)
            .AsQueryable();

        if (filter.Category.HasValue)
            query = query.Where(p => p.Category == filter.Category.Value);

        if (filter.Featured.HasValue)
            query = query.Where(p => p.Featured == filter.Featured.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{filter.Search}%") ||
                EF.Functions.ILike(p.Description, $"%{filter.Search}%"));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.Featured)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<List<string>> GetAllSlugsAsync(CancellationToken ct) =>
        db.Products.Where(p => p.IsActive).Select(p => p.Slug).ToListAsync(ct);

    public Task AddAsync(Product product, CancellationToken ct) =>
        db.Products.AddAsync(product, ct).AsTask();

    public void Update(Product product) => db.Products.Update(product);

    public void Delete(Product product) => db.Products.Remove(product);
}
