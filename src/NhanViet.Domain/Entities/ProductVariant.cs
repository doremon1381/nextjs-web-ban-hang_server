using NhanViet.Domain.Exceptions;

namespace NhanViet.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal? OldPrice { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Product Product { get; private set; } = null!;

    public bool IsOnSale => OldPrice.HasValue && OldPrice.Value > Price;
    public int DiscountPercent => IsOnSale
        ? (int)Math.Round((OldPrice!.Value - Price) / OldPrice.Value * 100)
        : 0;
    public bool InStock => Stock > 0;

    public static ProductVariant Create(Guid productId, string name, decimal price, decimal? oldPrice, int stock) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Name = name,
            Price = price,
            OldPrice = oldPrice,
            Stock = stock,
            IsActive = true,
        };

    public void DeductStock(int quantity)
    {
        if (quantity > Stock)
            throw new InsufficientStockException(ProductId, Id, Stock, quantity);
        Stock -= quantity;
    }

    public void RestoreStock(int quantity) => Stock += quantity;

    public void Update(string name, decimal price, decimal? oldPrice, int stock)
    {
        Name = name;
        Price = price;
        OldPrice = oldPrice;
        Stock = stock;
    }
}
