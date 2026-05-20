using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhanViet.Application.Auth.Queries;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Application.Contact.Commands;
using NhanViet.Application.Orders.Commands;
using NhanViet.Domain.Interfaces;
using NhanViet.Infrastructure.Data;
using NhanViet.Infrastructure.Repositories;
using NhanViet.Infrastructure.Services;

namespace NhanViet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NhanVietDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("SupabaseDirectConnection"),
                npgsql => npgsql.MigrationsAssembly("NhanViet.Infrastructure")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NhanVietDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IGuestSessionTokenService, GuestSessionTokenService>();
        services.AddScoped<IShippingCalculator, ShippingCalculator>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<IOrderCreationService, OrderCreationService>();
        services.AddScoped<IOrderCancellationService, OrderCancellationService>();

        services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
        services.AddHostedService<BackgroundJobWorker>();

        services.AddHttpClient<IFileStorageService, SupabaseStorageService>();
        services.AddHttpClient<ISupabaseAdminService, SupabaseAdminService>();

        return services;
    }
}
