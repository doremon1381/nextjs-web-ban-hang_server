namespace NhanViet.Application.Common.Interfaces;

public interface IIdempotencyStore
{
    Task<string?> TryGetAsync(string key, CancellationToken ct = default);
    Task SaveAsync(string key, string responseJson, CancellationToken ct = default);
}
