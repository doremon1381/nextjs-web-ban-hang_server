namespace NhanViet.Application.Contact.DTOs;

public record ContactMessageDto(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    string Subject,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);

public record SubmitContactRequest(
    string Name,
    string Phone,
    string Email,
    string Subject,
    string Message
);
