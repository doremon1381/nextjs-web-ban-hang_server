using NhanViet.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace NhanViet.Application.Orders.Commands;

public record SendOrderConfirmationJob(string Email, string OrderCode, decimal Total) : IBackgroundJob
{
    public async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken ct)
    {
        var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
        await emailSender.SendOrderConfirmationAsync(Email, OrderCode, Total);
    }
}
