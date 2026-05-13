using MediatR;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Application.Common.Models;
using NhanViet.Application.Orders.DTOs;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Orders.Queries;

public record ListOrdersQuery(
    OrderStatus? Status,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderDto>>;

public class ListOrdersHandler(
    IOrderRepository orders,
    ICurrentUserService currentUser
) : IRequestHandler<ListOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(ListOrdersQuery req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var (items, total) = await orders.ListByUserAsync(
            currentUser.UserId!.Value, req.Status, req.Page, req.PageSize, ct);

        return new PagedResult<OrderDto>(
            items.Select(o => o.ToDto()).ToList(),
            total, req.Page, req.PageSize
        );
    }
}

public record ListAllOrdersQuery(
    OrderStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderDto>>;

public class ListAllOrdersHandler(
    IOrderRepository orders,
    ICurrentUserService currentUser
) : IRequestHandler<ListAllOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(ListAllOrdersQuery req, CancellationToken ct)
    {
        if (!currentUser.IsAdmin) throw new UnauthorizedAccessException();

        var (items, total) = await orders.ListAllAsync(req.Status, req.Page, req.PageSize, ct);
        return new PagedResult<OrderDto>(
            items.Select(o => o.ToDto()).ToList(),
            total, req.Page, req.PageSize
        );
    }
}
