using MongoDB.Entities;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure.Services;

public class MongoDbInitializer
{
    private readonly MongoDbSettings _mongoDbSettings;
    private static bool _initialized = false;
    private static readonly object _lock = new object();

    public MongoDbInitializer(MongoDbSettings mongoDbSettings)
    {
        _mongoDbSettings = mongoDbSettings;
    }

    public async Task InitializeAsync()
    {
        if (!_initialized)
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    InitializeMongoDb().GetAwaiter().GetResult();
                    _initialized = true;
                }
            }
        }
    }

    private async Task InitializeMongoDb()
    {
        try
        {
            Console.WriteLine($"Inicializando MongoDB: {_mongoDbSettings.DatabaseName}");

            await DB.InitAsync(
                _mongoDbSettings.DatabaseName,
                MongoDB.Driver.MongoClientSettings.FromConnectionString(_mongoDbSettings.ConnectionString)
            );

            // Configurar convenções e índices
            await ConfigureMongoDb();

            Console.WriteLine("MongoDB inicializado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar MongoDB: {ex.Message}");
            throw;
        }
    }

    private async Task ConfigureMongoDb()
    {
        // Configurar índices para melhor performance
        await DB.Index<Domain.Entities.User>()
            .Key(u => u.Email, KeyType.Ascending)
            .Option(o => o.Unique = true)
            .CreateAsync();

        await DB.Index<Domain.Entities.Transaction>()
            .Key(t => t.UserId, KeyType.Ascending)
            .Key(t => t.Date, KeyType.Descending)
            .CreateAsync();

        await DB.Index<Domain.Entities.Transaction>()
            .Key(t => t.UserId, KeyType.Ascending)
            .Key(t => t.Category, KeyType.Ascending)
            .CreateAsync();

        await DB.Index<Domain.Entities.Budget>()
            .Key(b => b.UserId, KeyType.Ascending)
            .Key(b => b.CategoryId, KeyType.Ascending)
            .CreateAsync();

        await DB.Index<Domain.Entities.Goal>()
            .Key(g => g.UserId, KeyType.Ascending)
            .Key(g => g.IsCompleted, KeyType.Ascending)
            .CreateAsync();
    }
}