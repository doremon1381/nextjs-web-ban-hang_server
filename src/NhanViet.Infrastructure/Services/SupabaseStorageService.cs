using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Infrastructure.Services;

public class SupabaseStorageService(HttpClient http, IConfiguration config) : IFileStorageService
{
    private string BaseUrl => config["Supabase:Url"]!;
    private string AnonKey => config["Supabase:AnonKey"]!;
    private string ServiceKey => config["Supabase:ServiceRoleKey"]!;

    public async Task<string> UploadAsync(Stream stream, string bucket, string path, string contentType, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/storage/v1/object/{bucket}/{path}";
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        req.Headers.Add("apikey", ServiceKey);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceKey);

        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        return GetPublicUrl(bucket, path);
    }

    public async Task DeleteAsync(string bucket, string path, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/storage/v1/object/{bucket}/{path}";
        using var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Add("apikey", ServiceKey);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceKey);
        await http.SendAsync(req, ct);
    }

    public string GetPublicUrl(string bucket, string path) =>
        $"{BaseUrl}/storage/v1/object/public/{bucket}/{path}";

    public async Task<string> GetSignedUrlAsync(string bucket, string path, int expiresInSeconds = 3600, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/storage/v1/object/sign/{bucket}/{path}";
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = System.Net.Http.Json.JsonContent.Create(new { expiresIn = expiresInSeconds })
        };
        req.Headers.Add("apikey", ServiceKey);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceKey);

        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadFromJsonAsync<SignedUrlResponse>(ct);
        return $"{BaseUrl}/storage/v1{json!.SignedUrl}";
    }

    private record SignedUrlResponse(string SignedUrl);
}
