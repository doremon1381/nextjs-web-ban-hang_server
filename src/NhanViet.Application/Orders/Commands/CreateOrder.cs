using System.Text.Json;
using FluentValidation;
using MediatR;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Application.Orders.DTOs;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Orders.Commands;

public record CreateOrderCommand(
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string ShippingAddress,
    PaymentMethod PaymentMethod,
    string? Note,
    string? IdempotencyKey
) : IRequest<OrderDto>;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CustomerPhone).NotEmpty().Matches(@"^0\d{9}$")
            .WithMessage("Số điện thoại không hợp lệ");
        RuleFor(x => x.CustomerEmail).EmailAddress().When(x => x.CustomerEmail is not null);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}

public class CreateOrderHandler(
    ICartRepository carts,
    IOrderRepository orders,
    ICurrentUserService currentUser,
    IShippingCalculator shipping,
    IIdempotencyStore idempotency,
    IBackgroundJobQueue jobs,
    IOrderCreationService orderCreation
) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand req, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(req.IdempotencyKey))
        {
            var cached = await idempotency.TryGetAsync(req.IdempotencyKey, ct);
            if (cached is not null)
                return JsonSerializer.Deserialize<OrderDto>(cached)!;
        }

        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var cart = await carts.GetByUserIdAsync(currentUser.UserId!.Value, ct);
        if (cart is null || cart.Items.Count == 0)
            throw new ValidationException("Giỏ hàng trống");

        var shippingFee = shipping.Calculate(cart.Subtotal);

        var order = await orderCreation.CreateOrderAtomicallyAsync(
            cart, req.CustomerName, req.CustomerPhone, req.CustomerEmail,
            req.ShippingAddress, req.PaymentMethod, req.Note, shippingFee, ct);

        if (req.CustomerEmail is not null)
            jobs.Enqueue(new SendOrderConfirmationJob(req.CustomerEmail, order.OrderCode, order.Total));

        var dto = order.ToDto();
        if (!string.IsNullOrEmpty(req.IdempotencyKey))
            await idempotency.SaveAsync(req.IdempotencyKey, JsonSerializer.Serialize(dto), ct);

        return dto;
    }
}

public interface IOrderCreationService
{
    Task<Order> CreateOrderAtomicallyAsync(
        Domain.Entities.Cart cart,
        string customerName, string customerPhone, string? customerEmail,
        string shippingAddress, PaymentMethod paymentMethod, string? note,
        decimal shippingFee, CancellationToken ct);
}
