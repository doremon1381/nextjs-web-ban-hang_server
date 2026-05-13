namespace NhanViet.Application.Common.Interfaces;

public interface ISupabaseAdminService
{
    Task SetUserRoleAsync(Guid userId, string role);
    Task DeleteUserAsync(Guid userId);
}
