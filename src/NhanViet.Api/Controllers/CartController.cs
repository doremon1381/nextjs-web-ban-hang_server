using MediatR;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Cart.Commands;
using NhanViet.Application.Cart.Queries;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/cart")]
public class CartController(IMediator mediator, IGuestSessionTokenService guestTokens) : ControllerBase
{
    private string? VerifiedGuestSession()
    {
        var raw = Request.Headers["X-NV-Session"].FirstOrDefault();
        return guestTokens.TryVerify(raw, out var sid) ? sid : null;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var cart = await mediator.Send(new GetCartQuery(VerifiedGuestSession()), ct);
        return cart is null ? Ok(new { items = Array.Empty<object>(), subtotal = 0, totalCount = 0 }) : Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddToCartCommand(req.ProductSlug, req.VariantId, req.Quantity, VerifiedGuestSession()), ct);

        if (result.IssuedSessionId is not null)
            Response.Headers["X-NV-Session"] = guestTokens.Issue(result.IssuedSessionId);

        return Ok(result.Cart);
    }

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateItemRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateCartItemCommand(cartItemId, req.Quantity, VerifiedGuestSession()), ct));

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken ct) =>
        Ok(await mediator.Send(new RemoveCartItemCommand(cartItemId, VerifiedGuestSession()), ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await mediator.Send(new ClearCartCommand(VerifiedGuestSession()), ct);
        return NoContent();
    }

    [HttpPost("merge")]
    public async Task<IActionResult> Merge(CancellationToken ct)
    {
        var guestSession = VerifiedGuestSession();
        if (guestSession is null) return BadRequest("Missing or invalid X-NV-Session.");
        return Ok(await mediator.Send(new MergeCartCommand(guestSession), ct));
    }
}

public record AddItemRequest(string ProductSlug, Guid VariantId, int Quantity);
public record UpdateItemRequest(int Quantity);
