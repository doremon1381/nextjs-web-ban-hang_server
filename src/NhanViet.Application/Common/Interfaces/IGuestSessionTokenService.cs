namespace NhanViet.Application.Common.Interfaces;

public interface IGuestSessionTokenService
{
    string Issue(string sessionId);

    bool TryVerify(string? token, out string sessionId);
}
