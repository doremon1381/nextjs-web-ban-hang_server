using FluentValidation;
using MediatR;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Domain.Entities;

namespace NhanViet.Application.Contact.Commands;

public record SubmitContactCommand(
    string Name, string Phone, string Email, string Subject, string Message
) : IRequest;

public class SubmitContactValidator : AbstractValidator<SubmitContactCommand>
{
    public SubmitContactValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^0\d{9}$").WithMessage("Số điện thoại không hợp lệ");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public interface IContactRepository
{
    Task AddAsync(ContactMessage message, CancellationToken ct = default);
    Task<(List<ContactMessage> Items, int TotalCount)> ListAsync(bool? isRead, int page, int pageSize, CancellationToken ct = default);
    Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Update(ContactMessage message);
}

public class SubmitContactHandler(
    IContactRepository contacts,
    IEmailSender emailSender,
    IUnitOfWork uow
) : IRequestHandler<SubmitContactCommand>
{
    public async Task Handle(SubmitContactCommand req, CancellationToken ct)
    {
        var message = ContactMessage.Create(req.Name, req.Phone, req.Email, req.Subject, req.Message);
        await contacts.AddAsync(message, ct);
        await uow.SaveChangesAsync(ct);
        await emailSender.SendContactNotificationAsync(req.Name, req.Email, req.Subject, req.Message);
    }
}
