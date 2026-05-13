using Microsoft.EntityFrameworkCore;
using NhanViet.Application.Common.Interfaces;
using NhanViet.Infrastructure.Data;

namespace NhanViet.Infrastructure.Services;

public class IdempotencyStore(NhanVietDbContext db) : IIdempotencyStore
{
    public async Task<string?> TryGetAsync(string key, CancellationToken ct = default)
    {
        var entry = await db.Database
            .SqlQueryRaw<IdempotencyEntry>(
                "SELECT key, response, created_at FROM public.idempotency_keys WHERE key = {0} AND created_at > NOW() - INTERVAL '24 hours'",
                key)
            .FirstOrDefaultAsync(ct);
        return entry?.Response;
    }

    public async Task SaveAsync(string key, string responseJson, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO public.idempotency_keys (key, response, created_at) VALUES ({0}, {1}, NOW()) ON CONFLICT (key) DO NOTHING",
            key, responseJson);
    }

    private record IdempotencyEntry(string Key, string Response, DateTime CreatedAt);
}
