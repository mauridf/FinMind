using FinMind.Domain.Entities;

namespace FinMind.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> GetByIdAsync(string id);
    Task<User> GetByEmailAsync(string email);
    Task<List<User>> GetAllAsync();
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsByEmailAsync(string email);
}