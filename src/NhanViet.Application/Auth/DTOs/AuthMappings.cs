using NhanViet.Domain.Entities;

namespace NhanViet.Application.Auth.DTOs;

public static class AuthMappings
{
    public static UserProfileDto ToDto(this AppUser u) => new(
        u.Id, u.Email, u.FullName, u.Phone, u.Address, u.AvatarUrl,
        u.Role, u.AuthProvider, u.CreatedAt
    );
}
