using MediatR;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Cart.Commands;

public record ClearCartCommand(string? SessionId) : IRequest;

public class ClearCartHandler(
    ICartRepository carts,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand req, CancellationToken ct)
    {
        var cart = currentUser.IsAuthenticated
            ? await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct)
            : req.SessionId is not null
                ? await carts.GetBySessionIdAsync(req.SessionId, ct)
                : null;

        if (cart is null) throw new NotFoundException("Cart", "current");

        cart.Clear();
        await uow.SaveChangesAsync(ct);
    }
}
