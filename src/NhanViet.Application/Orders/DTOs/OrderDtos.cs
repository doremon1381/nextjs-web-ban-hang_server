using NhanViet.Domain.Enums;

namespace NhanViet.Application.Orders.DTOs;

public record OrderDto(
    Guid Id,
    string OrderCode,
    string Status,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    string PaymentMethod,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string ShippingAddress,
    string? Note,
    DateTime CreatedAt,
    List<OrderItemDto> Items,
    List<OrderStatusHistoryDto> StatusHistory
);

public record OrderItemDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);

public record OrderStatusHistoryDto(
    string Status,
    DateTime Timestamp,
    string? Reason
);
