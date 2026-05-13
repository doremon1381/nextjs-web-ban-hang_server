using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Infrastructure.Services;

public class ShippingCalculator : IShippingCalculator
{
    private const decimal FreeShippingThreshold = 499_000m;
    private const decimal StandardShippingFee = 30_000m;

    public decimal Calculate(decimal subtotal) =>
        subtotal >= FreeShippingThreshold ? 0m : StandardShippingFee;
}
