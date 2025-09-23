using Microsoft.Extensions.DependencyInjection;
using FinMind.Application.Services;

namespace FinMind.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<GoalService>();
        services.AddScoped<AuthService>(); // Adicionar AuthService

        return services;
    }
}