using NhanViet.Domain.Exceptions;

namespace NhanViet.Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string? SessionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    private readonly List<CartItem> _items = [];

    public decimal Subtotal => _items.Sum(i => i.LineTotal);
    public int TotalCount => _items.Sum(i => i.Quantity);

    public static Cart CreateForUser(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    public static Cart CreateForGuest(string sessionId) => new()
    {
        Id = Guid.NewGuid(),
        SessionId = sessionId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    public void AddItem(Product product, ProductVariant variant, int quantity)
    {
        if (quantity < 1) throw new ArgumentException("Quantity must be >= 1");
        if (variant.Stock < quantity)
            throw new InsufficientStockException(product.Id, variant.Id, variant.Stock, quantity);

        var existing = _items.FirstOrDefault(i =>
            i.ProductId == product.Id && i.VariantId == variant.Id);

        if (existing is not null)
            existing.UpdateQuantity(existing.Quantity + quantity, variant.Stock);
        else
            _items.Add(CartItem.Create(product, variant, quantity));

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is not null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateItemQuantity(Guid cartItemId, int quantity, int maxStock)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new CartItemNotFoundException(Guid.Empty, Guid.Empty);
        item.UpdateQuantity(quantity, maxStock);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MergeFrom(Cart guest, IReadOnlyDictionary<Guid, int> currentStockByVariant)
    {
        foreach (var item in guest.Items)
        {
            if (!currentStockByVariant.TryGetValue(item.VariantId, out var stock))
                continue;

            var existing = _items.FirstOrDefault(i =>
                i.ProductId == item.ProductId && i.VariantId == item.VariantId);
            if (existing is not null)
                existing.UpdateQuantity(Math.Min(existing.Quantity + item.Quantity, stock), stock);
            else
                _items.Add(item);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToUser(Guid userId)
    {
        UserId = userId;
        SessionId = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
