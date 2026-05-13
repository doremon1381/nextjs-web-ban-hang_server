using NhanViet.Domain.Entities;

namespace NhanViet.Application.Products.DTOs;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product p) => new(
        p.Id, p.Slug, p.Name, p.Description,
        p.Price, p.OldPrice, p.Unit, p.Image,
        p.Category.ToString().ToLowerInvariant(),
        p.Badge, p.Rating, p.Featured
    );

    public static ProductDetailDto ToDetailDto(this Product p) => new(
        p.Id, p.Slug, p.Name, p.Description, p.FullDescription,
        p.Price, p.OldPrice, p.Unit, p.Image, p.Images,
        p.Category.ToString().ToLowerInvariant(),
        p.Badge, p.Rating, p.Featured,
        p.Origin, p.Harvest, p.Packaging, p.Storage, p.Shipping,
        p.Variants.Select(v => v.ToDto()).ToList()
    );

    public static VariantDto ToDto(this ProductVariant v) => new(
        v.Id, v.Name, v.Price, v.OldPrice, v.Stock, v.InStock, v.DiscountPercent
    );
}
