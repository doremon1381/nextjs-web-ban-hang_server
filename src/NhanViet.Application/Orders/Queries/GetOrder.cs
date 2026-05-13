using MediatR;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Application.Orders.DTOs;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Orders.Queries;

public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto>;

public class GetOrderHandler(
    IOrderRepository orders,
    ICurrentUserService currentUser
) : IRequestHandler<GetOrderQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderQuery req, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(req.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), req.OrderId);

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
            throw new UnauthorizedAccessException();

        return order.ToDto();
    }
}
