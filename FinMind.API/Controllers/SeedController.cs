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

    [HttpPost("reset")]
    public async Task<ActionResult> ResetData()
    {
        // Permitir apenas em ambiente de desenvolvimento
        if (!_environment.IsDevelopment())
        {
            return BadRequest(new { error = "Reset de dados permitido apenas em desenvolvimento" });
        }

        try
        {
            // Usar reflection para chamar método ClearAndSeedDataAsync se existir
            var method = _seedService.GetType().GetMethod("ClearAndSeedDataAsync");
            if (method != null)
            {
                await (Task)method.Invoke(_seedService, null);
                return Ok(new { message = "Dados resetados e recriados com sucesso" });
            }
            else
            {
                await _seedService.SeedDataAsync();
                return Ok(new { message = "Seed executado (dados mantidos se existirem)" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}