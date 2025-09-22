using Microsoft.AspNetCore.Mvc;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult Get()
    {
        return Ok(new
        {
            status = "API está funcionando",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}