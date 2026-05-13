namespace NhanViet.Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string VariantName { get; private set; } = string.Empty;
    public string ProductImage { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public static CartItem Create(Product product, ProductVariant variant, int quantity) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = product.Id,
        VariantId = variant.Id,
        ProductName = product.Name,
        VariantName = variant.Name,
        ProductImage = product.Image,
        Unit = product.Unit,
        UnitPrice = variant.Price,
        Quantity = quantity,
    };

    public void UpdateQuantity(int quantity, int maxStock)
    {
        if (quantity < 1) throw new ArgumentException("Quantity must be >= 1");
        Quantity = Math.Min(quantity, maxStock);
    }
}
