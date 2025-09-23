using Microsoft.AspNetCore.Mvc;
using FinMind.Application.Interfaces.Services;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly ISeedService _seedService;
    private readonly IWebHostEnvironment _environment;

    public SeedController(ISeedService seedService, IWebHostEnvironment environment)
    {
        _seedService = seedService;
        _environment = environment;
    }

    [HttpPost]
    public async Task<ActionResult> SeedData()
    {
        // Permitir apenas em ambiente de desenvolvimento
        if (!_environment.IsDevelopment())
        {
            return BadRequest(new { error = "Seed permitido apenas em ambiente de desenvolvimento" });
        }

        try
        {
            await _seedService.SeedDataAsync();
            return Ok(new { message = "Dados de seed criados com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}