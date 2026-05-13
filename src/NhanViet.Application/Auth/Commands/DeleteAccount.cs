using MediatR;
using NhanViet.Application.Auth.Queries;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Application.Auth.Commands;

public record DeleteAccountCommand : IRequest;

public class DeleteAccountHandler(
    IAppUserRepository users,
    ICurrentUserService currentUser,
    ISupabaseAdminService supabaseAdmin,
    IUnitOfWork uow
) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var user = await users.GetByIdAsync(currentUser.UserId!.Value, ct)
            ?? throw new NotFoundException("AppUser", currentUser.UserId!.Value);

        users.Delete(user);
        await uow.SaveChangesAsync(ct);
        await supabaseAdmin.DeleteUserAsync(currentUser.UserId!.Value);
    }
}
