using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly MongoDbSettings _mongoDbSettings;
    public BudgetRepository(MongoDbSettings mongoDbSettings)
    {
        _mongoDbSettings = mongoDbSettings;

        // Configurar o MongoDB.Entities uma única vez
        Task.Run(async () => await DB.InitAsync(
            _mongoDbSettings.DatabaseName,
            MongoDB.Driver.MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString)
        )).GetAwaiter().GetResult();
    }

    public async Task<Budget> GetByIdAsync(string id)
    {
        return await DB.Find<Budget>().OneAsync(id);
    }

    public async Task<List<Budget>> GetByUserIdAsync(string userId)
    {
        return await DB.Find<Budget>()
            .Match(b => b.UserId == userId)
            .Sort(b => b.CreatedAt, MongoDB.Entities.Order.Descending)
            .ExecuteAsync();
    }

    public async Task<List<Budget>> GetActiveBudgetsAsync(string userId)
    {
        var now = DateTime.UtcNow;
        return await DB.Find<Budget>()
            .Match(b => b.UserId == userId && b.StartDate <= now && b.EndDate >= now)
            .ExecuteAsync();
    }

    public async Task<Budget> AddAsync(Budget budget)
    {
        await DB.SaveAsync(budget);
        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        await DB.Update<Budget>()
            .MatchID(budget.ID)
            .ModifyWith(budget)
            .ExecuteAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await DB.DeleteAsync<Budget>(id);
    }

    public async Task<Budget> GetByCategoryAsync(string userId, string categoryId)
    {
        return await DB.Find<Budget>()
            .Match(b => b.UserId == userId && b.CategoryId == categoryId)
            .ExecuteFirstAsync();
    }
}