using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Domain.Entities;

namespace FinMind.Application.Services;

public class GoalService
{
    private readonly IGoalRepository _goalRepository;

    public GoalService(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto> GetGoalByIdAsync(string id)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null) throw new ArgumentException("Meta não encontrada");

        return MapToDto(goal);
    }

    public async Task<List<GoalDto>> GetUserGoalsAsync(string userId)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId);
        return goals.Select(MapToDto).ToList();
    }

    public async Task<List<GoalDto>> GetActiveGoalsAsync(string userId)
    {
        var goals = await _goalRepository.GetActiveGoalsAsync(userId);
        return goals.Select(MapToDto).ToList();
    }

    public async Task<List<GoalDto>> GetCompletedGoalsAsync(string userId)
    {
        var goals = await _goalRepository.GetCompletedGoalsAsync(userId);
        return goals.Select(MapToDto).ToList();
    }

    public async Task<GoalDto> CreateGoalAsync(string userId, CreateGoalDto createGoalDto)
    {
        if (createGoalDto.TargetDate <= DateTime.UtcNow)
            throw new InvalidOperationException("A data alvo deve ser futura");

        var goal = new Goal
        {
            UserId = userId,
            Name = createGoalDto.Name,
            TargetAmount = createGoalDto.TargetAmount,
            TargetDate = createGoalDto.TargetDate,
            Type = createGoalDto.Type,
            Priority = createGoalDto.Priority
        };

        var createdGoal = await _goalRepository.AddAsync(goal);
        return MapToDto(createdGoal);
    }

    public async Task<GoalDto> UpdateGoalAsync(string id, UpdateGoalDto updateGoalDto)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null) throw new ArgumentException("Meta não encontrada");

        if (goal.IsCompleted)
            throw new InvalidOperationException("Não é possível alterar uma meta concluída");

        goal.Name = updateGoalDto.Name;
        goal.TargetAmount = updateGoalDto.TargetAmount;
        goal.TargetDate = updateGoalDto.TargetDate;
        goal.Priority = updateGoalDto.Priority;

        await _goalRepository.UpdateAsync(goal);
        return MapToDto(goal);
    }

    public async Task<GoalDto> UpdateGoalProgressAsync(string id, UpdateGoalProgressDto progressDto)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null) throw new ArgumentException("Meta não encontrada");

        if (goal.IsCompleted)
            throw new InvalidOperationException("Meta já está concluída");

        goal.CurrentAmount = progressDto.Amount;

        // Verificar se a meta foi concluída
        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.IsCompleted = true;
        }

        await _goalRepository.UpdateAsync(goal);
        return MapToDto(goal);
    }

    public async Task DeleteGoalAsync(string id)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null) throw new ArgumentException("Meta não encontrada");

        await _goalRepository.DeleteAsync(id);
    }

    public async Task<GoalDto> CompleteGoalAsync(string id)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null) throw new ArgumentException("Meta não encontrada");

        goal.CurrentAmount = goal.TargetAmount;
        goal.IsCompleted = true;

        await _goalRepository.UpdateAsync(goal);
        return MapToDto(goal);
    }

    private static GoalDto MapToDto(Goal goal)
    {
        var daysRemaining = (goal.TargetDate - DateTime.UtcNow).Days;

        return new GoalDto
        {
            Id = goal.ID,
            UserId = goal.UserId,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            TargetDate = goal.TargetDate,
            Type = goal.Type,
            Priority = goal.Priority,
            Progress = goal.Progress,
            IsCompleted = goal.IsCompleted,
            IsNearDueDate = goal.IsNearDueDate(),
            DaysRemaining = daysRemaining > 0 ? daysRemaining : 0,
            CreatedAt = goal.CreatedAt,
            UpdatedAt = goal.UpdatedAt
        };
    }
}