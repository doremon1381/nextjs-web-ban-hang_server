using Microsoft.Extensions.Configuration;
using NhanViet.Application.Common.Interfaces;
using System.Net.Http.Headers;

namespace NhanViet.Infrastructure.Services;

public class SupabaseAdminService(HttpClient http, IConfiguration config) : ISupabaseAdminService
{
    public async Task SetUserRoleAsync(Guid userId, string role)
    {
        using var req = BuildAdminRequest(HttpMethod.Put, $"admin/users/{userId}");
        req.Content = System.Net.Http.Json.JsonContent.Create(new { app_metadata = new { role } });
        (await http.SendAsync(req)).EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        using var req = BuildAdminRequest(HttpMethod.Delete, $"admin/users/{userId}");
        (await http.SendAsync(req)).EnsureSuccessStatusCode();
    }

    private HttpRequestMessage BuildAdminRequest(HttpMethod method, string path)
    {
        var supabaseUrl = config["Supabase:Url"]!;
        var serviceKey = config["Supabase:ServiceRoleKey"]!;
        var req = new HttpRequestMessage(method, $"{supabaseUrl}/auth/v1/{path}");
        req.Headers.Add("apikey", serviceKey);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
        return req;
    }
}
