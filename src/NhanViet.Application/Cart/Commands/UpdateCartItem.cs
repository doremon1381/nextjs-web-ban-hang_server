using MediatR;
using NhanViet.Application.Cart.DTOs;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Cart.Commands;

public record UpdateCartItemCommand(
    Guid CartItemId,
    int Quantity,
    string? SessionId
) : IRequest<CartDto>;

public class UpdateCartItemHandler(
    ICartRepository carts,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    public async Task<CartDto> Handle(UpdateCartItemCommand req, CancellationToken ct)
    {
        var cart = await GetCartAsync(req.SessionId, ct)
            ?? throw new NotFoundException("Cart", req.CartItemId);

        var item = cart.Items.FirstOrDefault(i => i.Id == req.CartItemId)
            ?? throw new NotFoundException("CartItem", req.CartItemId);

        cart.UpdateItemQuantity(req.CartItemId, req.Quantity, item.Quantity + 9999);
        await uow.SaveChangesAsync(ct);
        return cart.ToDto();
    }

    private async Task<Domain.Entities.Cart?> GetCartAsync(string? sessionId, CancellationToken ct) =>
        currentUser.IsAuthenticated
            ? await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct)
            : sessionId is not null
                ? await carts.GetBySessionIdAsync(sessionId, ct)
                : null;
}
