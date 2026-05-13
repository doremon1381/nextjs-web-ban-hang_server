using MediatR;
using NhanViet.Application.Common.Models;
using NhanViet.Application.Products.DTOs;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Products.Queries;

public record ListProductsQuery(
    ProductCategory? Category,
    bool? Featured,
    string? Search,
    int Page = 1,
    int PageSize = 12
) : IRequest<PagedResult<ProductDto>>;

public class ListProductsHandler(IProductRepository products)
    : IRequestHandler<ListProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(ListProductsQuery req, CancellationToken ct)
    {
        var filter = new ProductFilter(req.Category, req.Featured, req.Search, req.Page, req.PageSize);
        var (items, total) = await products.ListAsync(filter, ct);
        return new PagedResult<ProductDto>(
            items.Select(p => p.ToDto()).ToList(),
            total, req.Page, req.PageSize
        );
    }
}
