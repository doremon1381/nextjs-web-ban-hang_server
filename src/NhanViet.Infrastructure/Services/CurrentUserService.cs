using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => User?.FindFirst("sub")?.Value is { } sub && Guid.TryParse(sub, out var id)
        ? id : null;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? Email => User?.FindFirst("email")?.Value;

    public string? AuthProvider => User?.FindFirst("provider")?.Value;

    // `role` is projected ONLY from app_metadata (service-role-writable) by the
    // JWT pipeline in Program.cs. user_metadata is namespaced under "umeta:" and
    // is NOT considered for authorization. The /admin/** routes additionally
    // cross-check against public.app_users.Role via DbRoleAuthorizationHandler.
    public bool IsAdmin => User?.FindFirst("role")?.Value == "Admin";
}
