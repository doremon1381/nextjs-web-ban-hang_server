namespace NhanViet.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendContactNotificationAsync(string name, string email, string subject, string message);
    Task SendOrderConfirmationAsync(string email, string orderCode, decimal total);
}
