namespace NhanViet.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string bucket, string path, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string path, CancellationToken ct = default);
    string GetPublicUrl(string bucket, string path);
    Task<string> GetSignedUrlAsync(string bucket, string path, int expiresInSeconds = 3600, CancellationToken ct = default);
}
