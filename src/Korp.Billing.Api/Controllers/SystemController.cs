using Microsoft.AspNetCore.Mvc;

namespace Korp.Billing.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", service = "billing" });
}
