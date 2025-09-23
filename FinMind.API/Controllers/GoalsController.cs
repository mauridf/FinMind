using Microsoft.AspNetCore.Mvc;
using FinMind.Application.DTOs;
using FinMind.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly GoalService _goalService;

    public GoalsController(GoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GoalDto>> GetGoal(string id)
    {
        try
        {
            var goal = await _goalService.GetGoalByIdAsync(id);
            return Ok(goal);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<GoalDto>>> GetUserGoals(string userId)
    {
        try
        {
            var goals = await _goalService.GetUserGoalsAsync(userId);
            return Ok(goals);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/active")]
    public async Task<ActionResult<List<GoalDto>>> GetActiveGoals(string userId)
    {
        try
        {
            var goals = await _goalService.GetActiveGoalsAsync(userId);
            return Ok(goals);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/completed")]
    public async Task<ActionResult<List<GoalDto>>> GetCompletedGoals(string userId)
    {
        try
        {
            var goals = await _goalService.GetCompletedGoalsAsync(userId);
            return Ok(goals);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("user/{userId}")]
    public async Task<ActionResult<GoalDto>> CreateGoal(string userId, CreateGoalDto createGoalDto)
    {
        try
        {
            var goal = await _goalService.CreateGoalAsync(userId, createGoalDto);
            return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GoalDto>> UpdateGoal(string id, UpdateGoalDto updateGoalDto)
    {
        try
        {
            var goal = await _goalService.UpdateGoalAsync(id, updateGoalDto);
            return Ok(goal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/progress")]
    public async Task<ActionResult<GoalDto>> UpdateGoalProgress(string id, UpdateGoalProgressDto progressDto)
    {
        try
        {
            var goal = await _goalService.UpdateGoalProgressAsync(id, progressDto);
            return Ok(goal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<GoalDto>> CompleteGoal(string id)
    {
        try
        {
            var goal = await _goalService.CompleteGoalAsync(id);
            return Ok(goal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGoal(string id)
    {
        try
        {
            await _goalService.DeleteGoalAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}