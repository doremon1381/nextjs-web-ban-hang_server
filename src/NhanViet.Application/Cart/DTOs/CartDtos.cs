namespace NhanViet.Application.Cart.DTOs;

public record CartDto(
    List<CartItemDto> Items,
    decimal Subtotal,
    int TotalCount
);

public record CartItemDto(
    Guid CartItemId,
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantName,
    string ProductImage,
    string Unit,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);
