using MediatR;
using NhanViet.Application.Cart.DTOs;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Cart.Commands;

public record AddToCartCommand(
    string ProductSlug,
    Guid VariantId,
    int Quantity,
    string? SessionId
) : IRequest<AddToCartResult>;

public record AddToCartResult(CartDto Cart, string? IssuedSessionId);

public class AddToCartHandler(
    ICartRepository carts,
    IProductRepository products,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<AddToCartCommand, AddToCartResult>
{
    public async Task<AddToCartResult> Handle(AddToCartCommand req, CancellationToken ct)
    {
        var product = await products.GetBySlugAsync(req.ProductSlug, ct)
            ?? throw new NotFoundException(nameof(Product), req.ProductSlug);

        var variant = product.Variants.FirstOrDefault(v => v.Id == req.VariantId)
            ?? throw new NotFoundException(nameof(ProductVariant), req.VariantId);

        string? issuedSessionId = null;
        Domain.Entities.Cart? cart;

        if (currentUser.IsAuthenticated)
        {
            cart = await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct);
            if (cart is null)
            {
                cart = Domain.Entities.Cart.CreateForUser(currentUser.UserId!.Value);
                await carts.AddAsync(cart, ct);
            }
        }
        else
        {
            var sessionId = req.SessionId;
            if (sessionId is null)
            {
                issuedSessionId = Guid.NewGuid().ToString();
                sessionId = issuedSessionId;
            }

            cart = await carts.GetBySessionIdAsync(sessionId, ct);
            if (cart is null)
            {
                cart = Domain.Entities.Cart.CreateForGuest(sessionId);
                await carts.AddAsync(cart, ct);
            }
        }

        cart.AddItem(product, variant, req.Quantity);
        await uow.SaveChangesAsync(ct);

        return new AddToCartResult(cart.ToDto(), issuedSessionId);
    }
}
