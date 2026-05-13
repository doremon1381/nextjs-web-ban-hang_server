using MediatR;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Orders.DTOs;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Orders.Commands;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<OrderDto>;

public class UpdateOrderStatusHandler(
    IOrderRepository orders,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    public async Task<OrderDto> Handle(UpdateOrderStatusCommand req, CancellationToken ct)
    {
        if (!currentUser.IsAdmin) throw new UnauthorizedAccessException();

        var order = await orders.GetByIdAsync(req.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), req.OrderId);

        switch (req.NewStatus)
        {
            case OrderStatus.Confirmed: order.Confirm(); break;
            case OrderStatus.Shipping: order.Ship(); break;
            case OrderStatus.Completed: order.Complete(); break;
            case OrderStatus.Cancelled: order.Cancel(); break;
            default: throw new ArgumentException($"Invalid target status: {req.NewStatus}");
        }

        orders.Update(order);
        await uow.SaveChangesAsync(ct);
        return order.ToDto();
    }
}
