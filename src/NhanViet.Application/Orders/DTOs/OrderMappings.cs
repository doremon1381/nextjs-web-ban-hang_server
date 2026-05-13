using NhanViet.Domain.Entities;

namespace NhanViet.Application.Orders.DTOs;

public static class OrderMappings
{
    public static OrderDto ToDto(this Order o) => new(
        o.Id, o.OrderCode,
        o.Status.ToString(),
        o.Subtotal, o.ShippingFee, o.Total,
        o.PaymentMethod.ToString(),
        o.CustomerName, o.CustomerPhone, o.CustomerEmail,
        o.ShippingAddress, o.Note, o.CreatedAt,
        o.Items.Select(i => i.ToDto()).ToList(),
        o.StatusHistory.Select(h => h.ToDto()).ToList()
    );

    public static OrderItemDto ToDto(this OrderItem i) => new(
        i.ProductId, i.VariantId,
        i.ProductName, i.VariantName,
        i.UnitPrice, i.Quantity, i.LineTotal
    );

    public static OrderStatusHistoryDto ToDto(this OrderStatusHistory h) => new(
        h.Status.ToString(), h.Timestamp, h.Reason
    );
}
