using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using FinMind.Application.Interfaces.Repositories;
using Microsoft.Extensions.Options;
using FinMind.Infrastructure.Data;
using Transaction = FinMind.Domain.Entities.Transaction;

namespace FinMind.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly MongoDbSettings _mongoDbSettings;
    public TransactionRepository(MongoDbSettings mongoDbSettings)
    {
        _mongoDbSettings = mongoDbSettings;

        // Configurar o MongoDB.Entities uma única vez
        Task.Run(async () => await DB.InitAsync(
            _mongoDbSettings.DatabaseName,
            MongoDB.Driver.MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString)
        )).GetAwaiter().GetResult();
    }

    public async Task<Transaction> GetByIdAsync(string id)
    {
        return await DB.Find<Transaction>().OneAsync(id);
    }

    public async Task<List<Transaction>> GetByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = DB.Find<Transaction>()
            .Match(t => t.UserId == userId);

        if (startDate.HasValue)
            query.Match(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query.Match(t => t.Date <= endDate.Value);

        return await query
            .Sort(t => t.Date, MongoDB.Entities.Order.Descending)
            .ExecuteAsync();
    }

    public async Task<List<Transaction>> GetByCategoryAsync(string userId, string category)
    {
        return await DB.Find<Transaction>()
            .Match(t => t.UserId == userId && t.Category == category)
            .Sort(t => t.Date, MongoDB.Entities.Order.Descending)
            .ExecuteAsync();
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        await DB.SaveAsync(transaction);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        await DB.Update<Transaction>()
            .MatchID(transaction.ID)
            .ModifyWith(transaction)
            .ExecuteAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await DB.DeleteAsync<Transaction>(id);
    }

    public async Task<decimal> GetTotalAmountByTypeAsync(string userId, TransactionType type, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = DB.Find<Transaction>()
            .Match(t => t.UserId == userId && t.Type == type && t.Status == TransactionStatus.Completed);

        if (startDate.HasValue)
            query.Match(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query.Match(t => t.Date <= endDate.Value);

        var transactions = await query.ExecuteAsync();
        return transactions.Sum(t => t.Amount);
    }
}