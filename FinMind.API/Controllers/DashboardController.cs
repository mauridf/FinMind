using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinMind.Application.DTOs;
using FinMind.Application.Services;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var summary = await _dashboardService.GetDashboardSummaryAsync(userId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("spending-by-category")]
    public async Task<ActionResult<List<CategorySpendingDto>>> GetSpendingByCategory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var spending = await _dashboardService.GetSpendingByCategoryAsync(userId, startDate, endDate);
            return Ok(spending);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<List<MonthlySummaryDto>>> GetMonthlySummary([FromQuery] int monthsBack = 6)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var summary = await _dashboardService.GetMonthlySummaryAsync(userId, monthsBack);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("cash-flow-projection")]
    public async Task<ActionResult<List<CashFlowProjectionDto>>> GetCashFlowProjection([FromQuery] int daysAhead = 30)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var projection = await _dashboardService.GetCashFlowProjectionAsync(userId, daysAhead);
            return Ok(projection);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("goals-progress")]
    public async Task<ActionResult<List<GoalProgressDto>>> GetGoalsProgress()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var progress = await _dashboardService.GetGoalsProgressAsync(userId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("budget-status")]
    public async Task<ActionResult<List<BudgetStatusDto>>> GetBudgetStatus()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var status = await _dashboardService.GetBudgetStatusAsync(userId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("financial-health")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetFinancialHealthMetrics()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var metrics = await _dashboardService.GetFinancialHealthMetricsAsync(userId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("quick-stats")]
    public async Task<ActionResult<object>> GetQuickStats()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var summary = await _dashboardService.GetDashboardSummaryAsync(userId);
            var spending = await _dashboardService.GetSpendingByCategoryAsync(userId);
            var budgets = await _dashboardService.GetBudgetStatusAsync(userId);
            var goals = await _dashboardService.GetGoalsProgressAsync(userId);

            var topSpending = spending.Take(3).ToList();
            var overBudget = budgets.Where(b => b.IsOverBudget).ToList();
            var nearBudget = budgets.Where(b => b.IsNearLimit).ToList();

            return Ok(new
            {
                Summary = summary,
                TopSpendingCategories = topSpending,
                OverBudgetCategories = overBudget,
                NearBudgetCategories = nearBudget,
                GoalsProgress = goals
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}