using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly MongoDbSettings _mongoDbSettings;
    public GoalRepository(MongoDbSettings mongoDbSettings)
    {
        _mongoDbSettings = mongoDbSettings;

        // Configurar o MongoDB.Entities uma única vez
        Task.Run(async () => await DB.InitAsync(
            _mongoDbSettings.DatabaseName,
            MongoDB.Driver.MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString)
        )).GetAwaiter().GetResult();
    }

    public async Task<Goal> GetByIdAsync(string id)
    {
        return await DB.Find<Goal>().OneAsync(id);
    }

    public async Task<List<Goal>> GetByUserIdAsync(string userId)
    {
        return await DB.Find<Goal>()
            .Match(g => g.UserId == userId)
            .Sort(g => g.CreatedAt, MongoDB.Entities.Order.Descending)
            .ExecuteAsync();
    }

    public async Task<List<Goal>> GetActiveGoalsAsync(string userId)
    {
        return await DB.Find<Goal>()
            .Match(g => g.UserId == userId && !g.IsCompleted)
            .ExecuteAsync();
    }

    public async Task<List<Goal>> GetCompletedGoalsAsync(string userId)
    {
        return await DB.Find<Goal>()
            .Match(g => g.UserId == userId && g.IsCompleted)
            .ExecuteAsync();
    }

    public async Task<Goal> AddAsync(Goal goal)
    {
        await DB.SaveAsync(goal);
        return goal;
    }

    public async Task UpdateAsync(Goal goal)
    {
        goal.UpdatedAt = DateTime.UtcNow;
        await DB.Update<Goal>()
            .MatchID(goal.ID)
            .ModifyWith(goal)
            .ExecuteAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await DB.DeleteAsync<Goal>(id);
    }

    public async Task UpdateProgressAsync(string goalId, decimal amount)
    {
        await DB.Update<Goal>()
            .MatchID(goalId)
            .Modify(g => g.CurrentAmount, amount)
            .Modify(g => g.UpdatedAt, DateTime.UtcNow)
            .ExecuteAsync();
    }
}