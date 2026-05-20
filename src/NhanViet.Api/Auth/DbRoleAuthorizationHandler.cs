using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Api.Auth;

public sealed class DbRoleRequirement(string requiredRole) : IAuthorizationRequirement
{
    public string RequiredRole { get; } = requiredRole;
}

public sealed class DbRoleAuthorizationHandler(IServiceScopeFactory scopeFactory)
    : AuthorizationHandler<DbRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DbRoleRequirement requirement)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(sub, out var userId)) return;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NhanVietDbContext>();

        var role = await db.AppUsers
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync();

        if (string.Equals(role, requirement.RequiredRole, StringComparison.Ordinal))
            context.Succeed(requirement);
    }
}
