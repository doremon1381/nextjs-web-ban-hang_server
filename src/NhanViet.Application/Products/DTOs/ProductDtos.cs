namespace NhanViet.Application.Products.DTOs;

public record ProductDto(
    Guid Id,
    string Slug,
    string Name,
    string Description,
    decimal Price,
    decimal? OldPrice,
    string Unit,
    string Image,
    string Category,
    string? Badge,
    decimal Rating,
    bool Featured
);

public record ProductDetailDto(
    Guid Id,
    string Slug,
    string Name,
    string Description,
    string? FullDescription,
    decimal Price,
    decimal? OldPrice,
    string Unit,
    string Image,
    List<string> Images,
    string Category,
    string? Badge,
    decimal Rating,
    bool Featured,
    string? Origin,
    string? Harvest,
    string? Packaging,
    string? Storage,
    string? Shipping,
    List<VariantDto> Variants
);

public record VariantDto(
    Guid Id,
    string Name,
    decimal Price,
    decimal? OldPrice,
    int Stock,
    bool InStock,
    int DiscountPercent
);

public record CreateProductRequest(
    string Slug,
    string Name,
    string Description,
    string? FullDescription,
    string Category,
    string Unit,
    string Image,
    List<string> Images,
    decimal Rating,
    string? Badge,
    bool Featured,
    string? Origin,
    string? Harvest,
    string? Packaging,
    string? Storage,
    string? Shipping,
    List<CreateVariantRequest> Variants
);

public record CreateVariantRequest(
    string Name,
    decimal Price,
    decimal? OldPrice,
    int Stock
);

public record UpdateProductRequest(
    string Name,
    string Description,
    string? FullDescription,
    string Category,
    string Unit,
    string Image,
    List<string> Images,
    decimal Rating,
    string? Badge,
    bool Featured,
    string? Origin,
    string? Harvest,
    string? Packaging,
    string? Storage,
    string? Shipping
);
