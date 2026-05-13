using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Auth.Commands;
using NhanViet.Application.Auth.DTOs;
using NhanViet.Application.Auth.Queries;
using NhanViet.Application.Common.Interfaces;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public class AuthController(IMediator mediator, IFileStorageService storage) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct) =>
        Ok(await mediator.Send(new GetCurrentUserQuery(), ct));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateProfileCommand(req.FullName, req.Phone, req.Address), ct));

    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        const long maxSize = 2 * 1024 * 1024;
        if (file.Length > maxSize) return BadRequest("File exceeds 2 MB limit");

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType)) return BadRequest("Only jpeg, png, webp allowed");

        var ext = Path.GetExtension(file.FileName);
        var path = $"{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        var url = await storage.UploadAsync(stream, "avatars", path, file.ContentType, ct);
        return Ok(new { avatarUrl = url });
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount(CancellationToken ct)
    {
        await mediator.Send(new DeleteAccountCommand(), ct);
        return NoContent();
    }
}
