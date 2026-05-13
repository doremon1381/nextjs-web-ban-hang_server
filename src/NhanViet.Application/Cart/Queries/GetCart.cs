using MediatR;
using NhanViet.Application.Cart.DTOs;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Cart.Queries;

public record GetCartQuery(string? SessionId) : IRequest<CartDto?>;

public class GetCartHandler(
    ICartRepository carts,
    ICurrentUserService currentUser
) : IRequestHandler<GetCartQuery, CartDto?>
{
    public async Task<CartDto?> Handle(GetCartQuery req, CancellationToken ct)
    {
        Domain.Entities.Cart? cart = currentUser.IsAuthenticated
            ? await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct)
            : req.SessionId is not null
                ? await carts.GetBySessionIdAsync(req.SessionId, ct)
                : null;

        return cart?.ToDto();
    }
}
