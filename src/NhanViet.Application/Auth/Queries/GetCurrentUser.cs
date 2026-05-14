using MediatR;
using NhanViet.Application.Auth.DTOs;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Application.Auth.Queries;

public record GetCurrentUserQuery : IRequest<UserProfileDto>;

public interface IAppUserRepository
{
    Task<Domain.Entities.AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Domain.Entities.AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<(List<Domain.Entities.AppUser> Items, int TotalCount)> ListAsync(
        string? search, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Domain.Entities.AppUser user, CancellationToken ct = default);
    void Update(Domain.Entities.AppUser user);
    void Delete(Domain.Entities.AppUser user);
}

public class GetCurrentUserHandler(
    IAppUserRepository users,
    ICurrentUserService currentUser
) : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetCurrentUserQuery req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedAccessException();

        var user = await users.GetByIdAsync(currentUser.UserId!.Value, ct)
            ?? throw new NotFoundException("AppUser", currentUser.UserId!.Value);

        return user.ToDto();
    }
}
