namespace NhanViet.Domain.Entities;

public class ContactMessage
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ContactMessage Create(string name, string phone, string email, string subject, string message) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            Email = email,
            Subject = subject,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };

    public void MarkAsRead() => IsRead = true;
}
