using NhanViet.Domain.Entities;

namespace NhanViet.Application.Cart.DTOs;

public static class CartMappings
{
    public static CartDto ToDto(this Domain.Entities.Cart c) => new(
        c.Items.Select(i => i.ToDto()).ToList(),
        c.Subtotal,
        c.TotalCount
    );

    public static CartItemDto ToDto(this CartItem i) => new(
        i.Id, i.ProductId, i.VariantId,
        i.ProductName, i.VariantName, i.ProductImage,
        i.Unit, i.UnitPrice, i.Quantity, i.LineTotal
    );
}
