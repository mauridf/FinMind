using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Application.Interfaces.Repositories;
using Microsoft.Extensions.Options;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbSettings _mongoDbSettings;

    public UserRepository(MongoDbSettings mongoDbSettings)
    {
        _mongoDbSettings = mongoDbSettings;

        // Configurar o MongoDB.Entities uma única vez
        Task.Run(async () => await DB.InitAsync(
            _mongoDbSettings.DatabaseName,
            MongoDB.Driver.MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString)
        )).GetAwaiter().GetResult();
    }

    public async Task<User> GetByIdAsync(string id)
    {
        return await DB.Find<User>().OneAsync(id);
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await DB.Find<User>()
            .Match(u => u.Email == email && u.IsActive)
            .ExecuteFirstAsync();
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await DB.Find<User>()
            .Match(u => u.IsActive)
            .ExecuteAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await DB.SaveAsync(user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await DB.Update<User>()
            .MatchID(user.ID)
            .ModifyWith(user)
            .ExecuteAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await DB.Update<User>()
            .MatchID(id)
            .Modify(u => u.IsActive, false)
            .ExecuteAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await DB.Find<User>()
            .Match(u => u.Email == email && u.IsActive)
            .ExecuteAnyAsync();
    }
}