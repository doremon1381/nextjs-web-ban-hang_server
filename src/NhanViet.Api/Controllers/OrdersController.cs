using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Orders.Commands;
using NhanViet.Application.Orders.Queries;
using NhanViet.Domain.Enums;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
        var paymentMethod = Enum.TryParse<PaymentMethod>(req.PaymentMethod, true, out var pm) ? pm : PaymentMethod.Cod;

        var result = await mediator.Send(new CreateOrderCommand(
            req.CustomerName, req.CustomerPhone, req.CustomerEmail,
            req.ShippingAddress, paymentMethod, req.Note, idempotencyKey), ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        OrderStatus? s = status is not null && Enum.TryParse<OrderStatus>(status, true, out var parsed) ? parsed : null;
        return Ok(await mediator.Send(new ListOrdersQuery(s, page, pageSize), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderQuery(id), ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest? req, CancellationToken ct)
    {
        await mediator.Send(new CancelOrderCommand(id, req?.Reason), ct);
        return NoContent();
    }
}

public record CreateOrderRequest(
    string CustomerName, string CustomerPhone, string? CustomerEmail,
    string ShippingAddress, string PaymentMethod, string? Note);

public record CancelRequest(string? Reason);
