using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly MongoDbSettings _mongoDbSettings;
    public CategoryRepository(MongoDbSettings mongoDbSettings)
    {
        _mongoDbSettings = mongoDbSettings;

        // Configurar o MongoDB.Entities uma única vez
        Task.Run(async () => await DB.InitAsync(
            _mongoDbSettings.DatabaseName,
            MongoDB.Driver.MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString)
        )).GetAwaiter().GetResult();
    }

    public async Task<Category> GetByIdAsync(string id)
    {
        return await DB.Find<Category>().OneAsync(id);
    }

    public async Task<List<Category>> GetByUserIdAsync(string userId)
    {
        return await DB.Find<Category>()
            .Match(c => c.UserId == userId || c.IsDefault)
            .Sort(c => c.Name, MongoDB.Entities.Order.Ascending)
            .ExecuteAsync();
    }

    public async Task<List<Category>> GetByTypeAsync(string userId, TransactionType type)
    {
        return await DB.Find<Category>()
            .Match(c => (c.UserId == userId || c.IsDefault) && c.Type == type)
            .ExecuteAsync();
    }

    public async Task<Category> AddAsync(Category category)
    {
        await DB.SaveAsync(category);
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        await DB.Update<Category>()
            .MatchID(category.ID)
            .ModifyWith(category)
            .ExecuteAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await DB.DeleteAsync<Category>(id);
    }

    public async Task<bool> ExistsByNameAsync(string userId, string name)
    {
        return await DB.Find<Category>()
            .Match(c => c.UserId == userId && c.Name.ToLower() == name.ToLower())
            .ExecuteAnyAsync();
    }

    public async Task<List<Category>> GetDefaultCategoriesAsync()
    {
        return await DB.Find<Category>()
            .Match(c => c.IsDefault)
            .ExecuteAsync();
    }
}