namespace NhanViet.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string VariantName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    public static OrderItem FromCartItem(CartItem ci) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = ci.ProductId,
        VariantId = ci.VariantId,
        ProductName = ci.ProductName,
        VariantName = ci.VariantName,
        UnitPrice = ci.UnitPrice,
        Quantity = ci.Quantity,
    };
}
