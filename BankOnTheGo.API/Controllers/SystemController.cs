using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    [HttpGet("check-ip")]
    public IActionResult GetIp()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var forwardedIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        return Ok(new { remoteIp, forwardedIp });
    }
    
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }
}