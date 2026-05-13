using System.Security.Cryptography;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Exceptions;

namespace NhanViet.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string OrderCode { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal Total { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string? CustomerEmail { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    private readonly List<OrderStatusHistory> _statusHistory = [];

    public static Order CreateFromCart(
        Cart cart,
        string customerName, string customerPhone, string? customerEmail,
        string shippingAddress, PaymentMethod paymentMethod, string? note,
        decimal shippingFee)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderCode = GenerateOrderCode(),
            UserId = cart.UserId,
            Status = OrderStatus.Pending,
            Subtotal = cart.Subtotal,
            ShippingFee = shippingFee,
            Total = cart.Subtotal + shippingFee,
            PaymentMethod = paymentMethod,
            Note = note,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail,
            ShippingAddress = shippingAddress,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var cartItem in cart.Items)
            order._items.Add(OrderItem.FromCartItem(cartItem));

        order._statusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = OrderStatus.Pending,
            Timestamp = DateTime.UtcNow,
        });

        return order;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Id, Status, OrderStatus.Confirmed);
        TransitionTo(OrderStatus.Confirmed);
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOrderStateException(Id, Status, OrderStatus.Shipping);
        TransitionTo(OrderStatus.Shipping);
    }

    public void Complete()
    {
        if (Status != OrderStatus.Shipping)
            throw new InvalidOrderStateException(Id, Status, OrderStatus.Completed);
        TransitionTo(OrderStatus.Completed);
    }

    public void Cancel(string? reason = null)
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new InvalidOrderStateException(Id, Status, OrderStatus.Cancelled);
        TransitionTo(OrderStatus.Cancelled, reason);
    }

    private void TransitionTo(OrderStatus newStatus, string? reason = null)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        _statusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = Id,
            Status = newStatus,
            Timestamp = DateTime.UtcNow,
            Reason = reason,
        });
    }

    private static string GenerateOrderCode()
    {
        const string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        Span<char> suffix = stackalloc char[6];
        Span<byte> bytes = stackalloc byte[6];
        RandomNumberGenerator.Fill(bytes);
        for (var i = 0; i < 6; i++) suffix[i] = alphabet[bytes[i] % 32];
        return $"DH{DateTime.UtcNow:yyMMdd}{new string(suffix)}";
    }
}
