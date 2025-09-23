using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Application.Interfaces.Services;
using FinMind.Infrastructure.Repositories;
using FinMind.Infrastructure.Data;

namespace FinMind.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar MongoDbSettings
        var mongoDbSettings = new MongoDbSettings
        {
            ConnectionString = configuration["MongoDbSettings:ConnectionString"] ?? "mongodb://localhost:27017",
            DatabaseName = configuration["MongoDbSettings:DatabaseName"] ?? "FinMind"
        };
        services.AddSingleton(mongoDbSettings);

        // Configurar JwtSettings
        var jwtSettings = new JwtSettings
        {
            Secret = configuration["JwtSettings:Secret"] ?? "ChavePadraoSuperSecretaParaDesenvolvimento32Caracteres!!",
            Issuer = configuration["JwtSettings:Issuer"] ?? "FinMindAPI",
            Audience = configuration["JwtSettings:Audience"] ?? "FinMindUsers",
            ExpirationMinutes = int.Parse(configuration["JwtSettings:ExpirationMinutes"] ?? "60"),
            RefreshTokenExpirationDays = int.Parse(configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7")
        };
        services.AddSingleton<IJwtSettings>(jwtSettings); // Registrar como IJwtSettings

        // Configurar Autenticação JWT
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret))
                };
            });

        // Registrar todos os repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();

        return services;
    }
}