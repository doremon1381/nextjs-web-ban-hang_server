using FluentValidation;
using MediatR;
using NhanViet.Application.Auth.DTOs;
using NhanViet.Application.Auth.Queries;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Application.Auth.Commands;

public record UpdateProfileCommand(string FullName, string? Phone, string? Address) : IRequest<UserProfileDto>;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).Matches(@"^0\d{9}$").When(x => x.Phone is not null)
            .WithMessage("Số điện thoại không hợp lệ");
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null);
    }
}

public class UpdateProfileHandler(
    IAppUserRepository users,
    ICurrentUserService currentUser,
    IUnitOfWork uow
) : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateProfileCommand req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var user = await users.GetByIdAsync(currentUser.UserId!.Value, ct)
            ?? throw new NotFoundException("AppUser", currentUser.UserId!.Value);

        user.FullName = req.FullName;
        user.Phone = req.Phone;
        user.Address = req.Address;
        user.UpdatedAt = DateTime.UtcNow;

        users.Update(user);
        await uow.SaveChangesAsync(ct);
        return user.ToDto();
    }
}
