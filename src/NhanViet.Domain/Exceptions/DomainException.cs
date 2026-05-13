namespace NhanViet.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public class InsufficientStockException(Guid productId, Guid variantId, int available, int requested)
    : DomainException($"Variant {variantId} of product {productId}: requested {requested}, available {available}");

public class InvalidOrderStateException(Guid orderId, Enums.OrderStatus current, Enums.OrderStatus target)
    : DomainException($"Order {orderId} cannot transition from {current} to {target}");

public class CartItemNotFoundException(Guid productId, Guid variantId)
    : DomainException($"Cart item not found: product {productId}, variant {variantId}");
