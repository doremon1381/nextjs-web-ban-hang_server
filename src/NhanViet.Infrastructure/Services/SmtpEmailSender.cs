using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NhanViet.Application.Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace NhanViet.Infrastructure.Services;

public class SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendContactNotificationAsync(string name, string email, string subject, string message)
    {
        try
        {
            using var mail = BuildMail(
                config["Email:SenderEmail"]!,
                $"[Nhãn Việt] Liên hệ mới: {subject}",
                $"Từ: {name} ({email})\n\n{message}");
            await SendAsync(mail);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send contact notification email");
        }
    }

    public async Task SendOrderConfirmationAsync(string email, string orderCode, decimal total)
    {
        try
        {
            using var mail = BuildMail(
                email,
                $"[Nhãn Việt] Xác nhận đơn hàng #{orderCode}",
                $"Cảm ơn bạn đã đặt hàng!\n\nMã đơn hàng: {orderCode}\nTổng tiền: {total:N0} VNĐ\n\nChúng tôi sẽ liên hệ xác nhận trong thời gian sớm nhất.");
            await SendAsync(mail);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send order confirmation to {Email}", email);
        }
    }

    private MailMessage BuildMail(string to, string subject, string body)
    {
        var senderEmail = config["Email:SenderEmail"] ?? "noreply@nhanviet.vn";
        var senderName = config["Email:SenderName"] ?? "Nhãn Việt";
        return new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            To = { to },
        };
    }

    private async Task SendAsync(MailMessage mail)
    {
        var host = config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.Parse(config["Email:SmtpPort"] ?? "587");
        var user = config["Email:SmtpUser"] ?? "";
        var pass = config["Email:SmtpPass"] ?? "";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass),
        };
        await client.SendMailAsync(mail);
    }
}
