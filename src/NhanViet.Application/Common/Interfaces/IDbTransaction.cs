namespace NhanViet.Application.Common.Interfaces;

public interface IDbTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}
