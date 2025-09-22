using FinMind.Domain.Entities;
using FinMind.Domain.Enums;

namespace FinMind.Application.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(string id);
    Task<List<Transaction>> GetByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<Transaction>> GetByCategoryAsync(string userId, string category);
    Task<Transaction> AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(string id);
    Task<decimal> GetTotalAmountByTypeAsync(string userId, TransactionType type, DateTime? startDate = null, DateTime? endDate = null);
}