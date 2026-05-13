using Microsoft.EntityFrameworkCore;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Api.Middleware;

public class UserProvisioningMiddleware(RequestDelegate next, ILogger<UserProvisioningMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx, NhanVietDbContext db)
    {
        if (ctx.User.Identity?.IsAuthenticated == true
            && Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var userId))
        {
            try
            {
                var email = ctx.User.FindFirst("email")?.Value ?? "";
                var fullName = ctx.User.FindFirst("full_name")?.Value ?? "";
                var avatar = ctx.User.FindFirst("avatar_url")?.Value;
                var provider = ctx.User.FindFirst("provider")?.Value ?? "email";

                await db.Database.ExecuteSqlAsync($@"
                    INSERT INTO public.app_users (id, email, full_name, avatar_url, auth_provider, role, created_at)
                    VALUES ({userId}, {email}, {fullName}, {avatar}, {provider}, 'Customer', NOW())
                    ON CONFLICT (id) DO NOTHING;");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AppUser provisioning failed for {UserId}", userId);
            }
        }
        await next(ctx);
    }
}
