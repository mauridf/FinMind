using FinMind.Domain.Entities;

namespace FinMind.Application.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<Goal> GetByIdAsync(string id);
    Task<List<Goal>> GetByUserIdAsync(string userId);
    Task<List<Goal>> GetActiveGoalsAsync(string userId);
    Task<List<Goal>> GetCompletedGoalsAsync(string userId);
    Task<Goal> AddAsync(Goal goal);
    Task UpdateAsync(Goal goal);
    Task DeleteAsync(string id);
    Task UpdateProgressAsync(string goalId, decimal amount);
}