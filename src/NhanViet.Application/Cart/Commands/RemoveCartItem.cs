using MediatR;
using NhanViet.Application.Cart.DTOs;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Cart.Commands;

public record RemoveCartItemCommand(Guid CartItemId, string? SessionId) : IRequest<CartDto>;

public class RemoveCartItemHandler(
    ICartRepository carts,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<RemoveCartItemCommand, CartDto>
{
    public async Task<CartDto> Handle(RemoveCartItemCommand req, CancellationToken ct)
    {
        var cart = currentUser.IsAuthenticated
            ? await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct)
            : req.SessionId is not null
                ? await carts.GetBySessionIdAsync(req.SessionId, ct)
                : null;

        if (cart is null) throw new NotFoundException("Cart", req.CartItemId);

        cart.RemoveItem(req.CartItemId);
        await uow.SaveChangesAsync(ct);
        return cart.ToDto();
    }
}
