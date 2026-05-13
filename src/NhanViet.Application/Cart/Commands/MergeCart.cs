using MediatR;
using NhanViet.Application.Cart.DTOs;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Cart.Commands;

public record MergeCartCommand(string GuestSessionId) : IRequest<CartDto>;

public class MergeCartHandler(
    ICartRepository carts,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<MergeCartCommand, CartDto>
{
    public async Task<CartDto> Handle(MergeCartCommand req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var userCart = await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct);
        if (userCart is null)
        {
            userCart = Domain.Entities.Cart.CreateForUser(currentUser.UserId!.Value);
            await carts.AddAsync(userCart, ct);
        }

        var guestCart = await carts.GetBySessionIdAsync(req.GuestSessionId, ct);
        if (guestCart is not null)
        {
            var stockByVariant = guestCart.Items
                .ToDictionary(i => i.VariantId, i => 9999); // stock validated at checkout
            userCart.MergeFrom(guestCart, stockByVariant);
            carts.Delete(guestCart);
        }

        await uow.SaveChangesAsync(ct);
        return userCart.ToDto();
    }
}
