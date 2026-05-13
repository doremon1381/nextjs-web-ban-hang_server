using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Products.Queries;
using NhanViet.Domain.Enums;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? category,
        [FromQuery] bool? featured,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        ProductCategory? cat = category is not null && Enum.TryParse<ProductCategory>(category, true, out var c) ? c : null;
        var result = await mediator.Send(new ListProductsQuery(cat, featured, search, page, Math.Min(pageSize, 50)), ct);
        return Ok(result);
    }

    [HttpGet("slugs")]
    public async Task<IActionResult> GetSlugs(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAllSlugsQuery(), ct));

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(CancellationToken ct)
    {
        var result = await mediator.Send(new ListProductsQuery(null, true, null, 1, 12), ct);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct) =>
        Ok(await mediator.Send(new GetProductBySlugQuery(slug), ct));
}
