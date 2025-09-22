using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Infrastructure.Repositories;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar MongoDbSettings CORRETAMENTE
        var mongoDbSettings = new MongoDbSettings();
        configuration.GetSection("MongoDbSettings").Bind(mongoDbSettings);
        services.AddSingleton(mongoDbSettings);

        // Registrar todos os repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();

        return services;
    }
}