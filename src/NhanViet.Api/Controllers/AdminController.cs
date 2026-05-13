using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Application.Contact.Commands;
using NhanViet.Application.Orders.Commands;
using NhanViet.Application.Orders.Queries;
using NhanViet.Application.Products.DTOs;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(
    IMediator mediator,
    IProductRepository productRepo,
    IContactRepository contactRepo,
    ISupabaseAdminService supabaseAdmin,
    IUnitOfWork uow
) : ControllerBase
{
    // --- Products ---

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var category = Enum.Parse<ProductCategory>(req.Category, true);
        var product = Product.Create(
            req.Slug, req.Name, req.Description, req.FullDescription,
            category, req.Unit, req.Image, req.Images,
            req.Rating, req.Badge, req.Featured,
            req.Origin, req.Harvest, req.Packaging, req.Storage, req.Shipping);

        foreach (var v in req.Variants)
            product.AddVariant(ProductVariant.Create(product.Id, v.Name, v.Price, v.OldPrice, v.Stock));

        await productRepo.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction("GetBySlug", "Products", new { slug = product.Slug }, product.ToDetailDto());
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);
        var category = Enum.Parse<ProductCategory>(req.Category, true);
        product.Update(req.Name, req.Description, req.FullDescription, category, req.Unit,
            req.Image, req.Images, req.Rating, req.Badge, req.Featured,
            req.Origin, req.Harvest, req.Packaging, req.Storage, req.Shipping);
        productRepo.Update(product);
        await uow.SaveChangesAsync(ct);
        return Ok(product.ToDetailDto());
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);
        product.Deactivate();
        productRepo.Update(product);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // --- Orders ---

    [HttpGet("orders")]
    public async Task<IActionResult> ListOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        OrderStatus? s = status is not null && Enum.TryParse<OrderStatus>(status, true, out var parsed) ? parsed : null;
        return Ok(await mediator.Send(new ListAllOrdersQuery(s, page, pageSize), ct));
    }

    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        var newStatus = Enum.Parse<OrderStatus>(req.Status, true);
        return Ok(await mediator.Send(new UpdateOrderStatusCommand(id, newStatus), ct));
    }

    // --- Contacts ---

    [HttpGet("contacts")]
    public async Task<IActionResult> ListContacts(
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (items, total) = await contactRepo.ListAsync(isRead, page, pageSize, ct);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPut("contacts/{id:guid}/read")]
    public async Task<IActionResult> MarkContactRead(Guid id, CancellationToken ct)
    {
        var msg = await contactRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContactMessage), id);
        msg.MarkAsRead();
        contactRepo.Update(msg);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // --- Role Management ---

    [HttpPut("users/{userId:guid}/role")]
    public async Task<IActionResult> SetUserRole(Guid userId, [FromBody] SetRoleRequest req, CancellationToken ct)
    {
        await supabaseAdmin.SetUserRoleAsync(userId, req.Role);
        return NoContent();
    }
}

public record UpdateStatusRequest(string Status);
public record SetRoleRequest(string Role);
