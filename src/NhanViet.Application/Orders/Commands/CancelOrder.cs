using MediatR;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderId, string? Reason) : IRequest;

public class CancelOrderHandler(
    IOrderRepository orders,
    ICurrentUserService currentUser,
    IOrderCancellationService orderCancellation
) : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand req, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(req.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), req.OrderId);

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
            throw new UnauthorizedAccessException();

        await orderCancellation.CancelOrderAtomicallyAsync(order, req.Reason, ct);
    }
}

public interface IOrderCancellationService
{
    Task CancelOrderAtomicallyAsync(Order order, string? reason, CancellationToken ct);
}
