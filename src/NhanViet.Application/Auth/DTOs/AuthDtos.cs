namespace NhanViet.Application.Auth.DTOs;

public record UserProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    string Role,
    string? AuthProvider,
    DateTime CreatedAt
);

public record UpdateProfileRequest(
    string FullName,
    string? Phone,
    string? Address
);
