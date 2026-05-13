using NhanViet.Domain.Enums;

namespace NhanViet.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? FullDescription { get; private set; }
    public ProductCategory Category { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string Image { get; private set; } = string.Empty;
    public List<string> Images { get; private set; } = [];
    public decimal Rating { get; private set; }
    public string? Badge { get; private set; }
    public bool Featured { get; private set; }
    public string? Origin { get; private set; }
    public string? Harvest { get; private set; }
    public string? Packaging { get; private set; }
    public string? Storage { get; private set; }
    public string? Shipping { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    private readonly List<ProductVariant> _variants = [];

    private ProductVariant? Anchor => _variants.Count > 0
        ? _variants.Where(v => v.IsActive).OrderBy(v => v.Price).FirstOrDefault()
        : null;

    public decimal Price => Anchor?.Price ?? 0;
    public decimal? OldPrice => Anchor?.OldPrice;

    public static Product Create(
        string slug, string name, string description, string? fullDescription,
        ProductCategory category, string unit, string image, List<string> images,
        decimal rating, string? badge, bool featured,
        string? origin, string? harvest, string? packaging, string? storage, string? shipping)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = name,
            Description = description,
            FullDescription = fullDescription,
            Category = category,
            Unit = unit,
            Image = image,
            Images = images,
            Rating = rating,
            Badge = badge,
            Featured = featured,
            Origin = origin,
            Harvest = harvest,
            Packaging = packaging,
            Storage = storage,
            Shipping = shipping,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string name, string description, string? fullDescription,
        ProductCategory category, string unit, string image, List<string> images,
        decimal rating, string? badge, bool featured,
        string? origin, string? harvest, string? packaging, string? storage, string? shipping)
    {
        Name = name;
        Description = description;
        FullDescription = fullDescription;
        Category = category;
        Unit = unit;
        Image = image;
        Images = images;
        Rating = rating;
        Badge = badge;
        Featured = featured;
        Origin = origin;
        Harvest = harvest;
        Packaging = packaging;
        Storage = storage;
        Shipping = shipping;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;

    public void AddVariant(ProductVariant variant) => _variants.Add(variant);
}
