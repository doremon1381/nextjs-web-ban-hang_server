using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Infrastructure.Services;

public sealed class GuestSessionTokenService : IGuestSessionTokenService
{
    private const int TagLengthBytes = 16;
    private readonly byte[] _key;

    public GuestSessionTokenService(IConfiguration config)
    {
        var secret = config["Security:GuestSessionSecret"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            throw new InvalidOperationException(
                "Security:GuestSessionSecret must be configured with at least 32 characters.");
        _key = Encoding.UTF8.GetBytes(secret);
    }

    public string Issue(string sessionId)
    {
        var tag = ComputeTag(sessionId);
        return $"{sessionId}.{Base64Url(tag)}";
    }

    public bool TryVerify(string? token, out string sessionId)
    {
        sessionId = string.Empty;
        if (string.IsNullOrEmpty(token)) return false;

        var dot = token.LastIndexOf('.');
        if (dot <= 0 || dot >= token.Length - 1) return false;

        var candidateId = token[..dot];
        var providedTag = token[(dot + 1)..];

        byte[] providedBytes;
        try { providedBytes = Base64UrlDecode(providedTag); }
        catch { return false; }
        if (providedBytes.Length != TagLengthBytes) return false;

        var expected = ComputeTag(candidateId);
        if (!CryptographicOperations.FixedTimeEquals(providedBytes, expected)) return false;

        sessionId = candidateId;
        return true;
    }

    private byte[] ComputeTag(string sessionId)
    {
        using var h = new HMACSHA256(_key);
        var full = h.ComputeHash(Encoding.UTF8.GetBytes("guest:" + sessionId));
        var truncated = new byte[TagLengthBytes];
        Buffer.BlockCopy(full, 0, truncated, 0, TagLengthBytes);
        return truncated;
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string s)
    {
        var padded = s.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }
}
