using FinMind.Domain.Entities;
using FinMind.Domain.Enums;

namespace FinMind.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category> GetByIdAsync(string id);
    Task<List<Category>> GetByUserIdAsync(string userId);
    Task<List<Category>> GetByTypeAsync(string userId, TransactionType type);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(string id);
    Task<bool> ExistsByNameAsync(string userId, string name);
    Task<List<Category>> GetDefaultCategoriesAsync();
}