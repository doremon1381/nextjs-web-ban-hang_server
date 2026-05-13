using MediatR;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Products.DTOs;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Products.Queries;

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto>;

public class GetProductBySlugHandler(IProductRepository products)
    : IRequestHandler<GetProductBySlugQuery, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(GetProductBySlugQuery req, CancellationToken ct)
    {
        var product = await products.GetBySlugAsync(req.Slug, ct)
            ?? throw new NotFoundException(nameof(Product), req.Slug);
        return product.ToDetailDto();
    }
}
