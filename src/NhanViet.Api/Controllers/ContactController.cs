using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NhanViet.Application.Contact.Commands;
using NhanViet.Application.Contact.DTOs;

namespace NhanViet.Api.Controllers;

[ApiController]
[Route("api/v1/contact")]
public class ContactController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Submit([FromBody] SubmitContactRequest req, CancellationToken ct)
    {
        await mediator.Send(new SubmitContactCommand(req.Name, req.Phone, req.Email, req.Subject, req.Message), ct);
        return NoContent();
    }
}
