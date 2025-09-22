using FinMind.Domain.Entities;

namespace FinMind.Application.Interfaces.Repositories;

public interface IBudgetRepository
{
    Task<Budget> GetByIdAsync(string id);
    Task<List<Budget>> GetByUserIdAsync(string userId);
    Task<List<Budget>> GetActiveBudgetsAsync(string userId);
    Task<Budget> AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(string id);
    Task<Budget> GetByCategoryAsync(string userId, string categoryId);
}