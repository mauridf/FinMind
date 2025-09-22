using Microsoft.AspNetCore.Mvc;
using FinMind.Application.DTOs;
using FinMind.Application.Services;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly BudgetService _budgetService;

    public BudgetsController(BudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetDto>> GetBudget(string id)
    {
        try
        {
            var budget = await _budgetService.GetBudgetByIdAsync(id);
            return Ok(budget);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<BudgetDto>>> GetUserBudgets(string userId)
    {
        try
        {
            var budgets = await _budgetService.GetUserBudgetsAsync(userId);
            return Ok(budgets);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/active")]
    public async Task<ActionResult<List<BudgetDto>>> GetActiveBudgets(string userId)
    {
        try
        {
            var budgets = await _budgetService.GetActiveBudgetsAsync(userId);
            return Ok(budgets);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("user/{userId}")]
    public async Task<ActionResult<BudgetDto>> CreateBudget(string userId, CreateBudgetDto createBudgetDto)
    {
        try
        {
            var budget = await _budgetService.CreateBudgetAsync(userId, createBudgetDto);
            return CreatedAtAction(nameof(GetBudget), new { id = budget.Id }, budget);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BudgetDto>> UpdateBudget(string id, UpdateBudgetDto updateBudgetDto)
    {
        try
        {
            var budget = await _budgetService.UpdateBudgetAsync(id, updateBudgetDto);
            return Ok(budget);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBudget(string id)
    {
        try
        {
            await _budgetService.DeleteBudgetAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/usage")]
    public async Task<ActionResult<decimal>> GetBudgetUsage(string id)
    {
        try
        {
            var usage = await _budgetService.CalculateBudgetUsageAsync(id);
            return Ok(new { budgetId = id, usagePercentage = usage });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}