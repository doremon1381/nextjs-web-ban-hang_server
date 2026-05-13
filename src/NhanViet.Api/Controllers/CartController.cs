using MediatR;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Cart.Commands;
using NhanViet.Application.Cart.Queries;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/cart")]
public class CartController(IMediator mediator) : ControllerBase
{
    private string? GuestSession => Request.Headers["X-NV-Session"].FirstOrDefault();

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var cart = await mediator.Send(new GetCartQuery(GuestSession), ct);
        return cart is null ? Ok(new { items = Array.Empty<object>(), subtotal = 0, totalCount = 0 }) : Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddToCartCommand(req.ProductSlug, req.VariantId, req.Quantity, GuestSession), ct);

        if (result.IssuedSessionId is not null)
            Response.Headers["X-NV-Session"] = result.IssuedSessionId;

        return Ok(result.Cart);
    }

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateItemRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateCartItemCommand(cartItemId, req.Quantity, GuestSession), ct));

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken ct) =>
        Ok(await mediator.Send(new RemoveCartItemCommand(cartItemId, GuestSession), ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await mediator.Send(new ClearCartCommand(GuestSession), ct);
        return NoContent();
    }

    [HttpPost("merge")]
    public async Task<IActionResult> Merge([FromBody] MergeCartRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new MergeCartCommand(req.GuestSessionId), ct));
}

public record AddItemRequest(string ProductSlug, Guid VariantId, int Quantity);
public record UpdateItemRequest(int Quantity);
public record MergeCartRequest(string GuestSessionId);
