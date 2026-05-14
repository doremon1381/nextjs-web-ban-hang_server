using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;

namespace NhanViet.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<(List<Product> Items, int TotalCount)> ListAsync(ProductFilter filter, CancellationToken ct = default);
    Task<List<string>> GetAllSlugsAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Delete(Product product);
}

public record ProductFilter(
    ProductCategory? Category = null,
    bool? Featured = null,
    string? Search = null,
    bool? IsActive = true,
    int Page = 1,
    int PageSize = 12
);
