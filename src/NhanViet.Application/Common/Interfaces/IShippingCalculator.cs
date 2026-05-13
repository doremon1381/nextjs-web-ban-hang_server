namespace NhanViet.Application.Common.Interfaces;

public interface IShippingCalculator
{
    decimal Calculate(decimal subtotal);
}
