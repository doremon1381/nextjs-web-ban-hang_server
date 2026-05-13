namespace NhanViet.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? AuthProvider { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
