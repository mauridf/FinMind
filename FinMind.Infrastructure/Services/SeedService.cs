using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using FinMind.Domain.ValueObjects;
using FinMind.Application.Interfaces.Services;
using Transaction = FinMind.Domain.Entities.Transaction;

namespace FinMind.Infrastructure.Services;

public class SeedService : ISeedService
{
    public async Task SeedDataAsync()
    {
        // Verificar se já existe dados para não duplicar
        var hasUsers = await DB.Find<User>().ExecuteAnyAsync();
        if (hasUsers)
        {
            Console.WriteLine("✅ Dados já existem no banco. Seed ignorado.");
            return;
        }

        Console.WriteLine("🌱 Iniciando seed de dados...");

        try
        {
            // 1. Criar categorias padrão do sistema
            var defaultCategories = await CreateDefaultCategoriesAsync();

            // 2. Criar usuários de exemplo
            var users = await CreateSampleUsersAsync();

            // 3. Para cada usuário, criar dados completos
            foreach (var user in users)
            {
                await CreateUserFinancialDataAsync(user, defaultCategories);
            }

            Console.WriteLine("✅ Seed de dados concluído com sucesso!");
            Console.WriteLine("👤 Usuário criado:");
            Console.WriteLine("   - mauridf@gmail.com / Mt190720@ (Dados completos)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro no seed: {ex.Message}");
            throw;
        }
    }

    private async Task<List<Category>> CreateDefaultCategoriesAsync()
    {
        var defaultCategories = new List<Category>
        {
            // RECEITAS
            new Category { Name = "Salário", Type = TransactionType.Income, Color = "#10B981", Icon = "💰", IsDefault = true },
            new Category { Name = "Freelance", Type = TransactionType.Income, Color = "#10B981", Icon = "💼", IsDefault = true },
            new Category { Name = "Investimentos", Type = TransactionType.Income, Color = "#10B981", Icon = "📈", IsDefault = true },
            new Category { Name = "Aluguel", Type = TransactionType.Income, Color = "#10B981", Icon = "🏠", IsDefault = true },
            new Category { Name = "Bonus", Type = TransactionType.Income, Color = "#10B981", Icon = "🎁", IsDefault = true },
            new Category { Name = "Outras Receitas", Type = TransactionType.Income, Color = "#10B981", Icon = "📥", IsDefault = true },

            // DESPESAS
            new Category { Name = "Moradia", Type = TransactionType.Expense, Color = "#EF4444", Icon = "🏠", IsDefault = true },
            new Category { Name = "Alimentação", Type = TransactionType.Expense, Color = "#F59E0B", Icon = "🍕", IsDefault = true },
            new Category { Name = "Transporte", Type = TransactionType.Expense, Color = "#3B82F6", Icon = "🚗", IsDefault = true },
            new Category { Name = "Saúde", Type = TransactionType.Expense, Color = "#8B5CF6", Icon = "🏥", IsDefault = true },
            new Category { Name = "Educação", Type = TransactionType.Expense, Color = "#06B6D4", Icon = "📚", IsDefault = true },
            new Category { Name = "Lazer", Type = TransactionType.Expense, Color = "#EC4899", Icon = "🎬", IsDefault = true },
            new Category { Name = "Vestuário", Type = TransactionType.Expense, Color = "#F97316", Icon = "👕", IsDefault = true },
            new Category { Name = "Telefonia/Internet", Type = TransactionType.Expense, Color = "#6366F1", Icon = "📱", IsDefault = true },
            new Category { Name = "Assinaturas", Type = TransactionType.Expense, Color = "#8B5CF6", Icon = "📋", IsDefault = true },
            new Category { Name = "Outras Despesas", Type = TransactionType.Expense, Color = "#6B7280", Icon = "📦", IsDefault = true }
        };

        await DB.SaveAsync(defaultCategories);
        Console.WriteLine($"✅ Criadas {defaultCategories.Count} categorias padrão");
        return defaultCategories;
    }

    private async Task<List<User>> CreateSampleUsersAsync()
    {
        var users = new List<User>
        {
            new User
            {
                Email = "mauridf@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mt190720@"),
                PersonalInfo = new PersonalInfo
                {
                    Name = "Maurício Dias de Carvalho Oliveira",
                    CPF = "793.311.571-34",
                    Phone = "(61) 99398-3844",
                    BirthDate = new DateTime(1977, 8, 30)
                },
                FinancialSettings = new FinancialSettings
                {
                    DefaultCurrency = "BRL",
                    MonthlyIncome = 20500m,
                    FinancialGoals = new List<string> { "Comprar um carro", "Quitar a Casa", "Fundo de emergência" }
                },
                Preferences = new UserPreferences
                {
                    Language = "pt-BR",
                    Notifications = true,
                    Theme = "dark"
                }
            }
        };

        await DB.SaveAsync(users);
        Console.WriteLine($"Criado {users.Count} usuário de exemplo");
        return users;
    }

    private async Task CreateUserFinancialDataAsync(User user, List<Category> categories)
    {
        Console.WriteLine($"Criando dados financeiros para: {user.PersonalInfo.Name}");

        // Criar transações dos últimos 6 meses
        await CreateSampleTransactionsAsync(user, categories);

        // Criar orçamentos
        await CreateSampleBudgetsAsync(user, categories);

        // Criar metas
        await CreateSampleGoalsAsync(user);

        // Criar categorias personalizadas
        await CreatePersonalCategoriesAsync(user);

        Console.WriteLine($"Dados financeiros criados para: {user.PersonalInfo.Name}");
    }

    private async Task CreateSampleTransactionsAsync(User user, List<Category> categories)
    {
        var transactions = new List<Transaction>();
        var random = new Random();
        var now = DateTime.UtcNow;

        // Diferentes perfis de gastos baseados no usuário
        var spendingProfile = user.Email switch
        {
            "mauridf@gmail.com" => "high",       // Alto gasto - muitos dados
            "joao.silva@email.com" => "medium",  // Médio gasto
            _ => "low"                           // Baixo gasto
        };

        // Criar transações dos últimos 6 meses
        for (int monthOffset = 0; monthOffset < 6; monthOffset++)
        {
            var monthDate = now.AddMonths(-monthOffset);
            var daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            // RECEITAS do mês
            var monthlyIncome = user.FinancialSettings.MonthlyIncome;
            transactions.Add(CreateTransaction(
                user.ID, TransactionType.Income, monthlyIncome, "Salário", "Salário", "Transferência",
                new DateTime(monthDate.Year, monthDate.Month, 5)));

            // Receitas extras ocasionais
            if (random.NextDouble() > 0.7) // 30% de chance
            {
                transactions.Add(CreateTransaction(
                    user.ID, TransactionType.Income, monthlyIncome * 0.2m, "Freelance", "Freelance", "PIX",
                    new DateTime(monthDate.Year, monthDate.Month, 15)));
            }

            // DESPESAS do mês - quantidade baseada no perfil
            var expenseCount = spendingProfile switch
            {
                "high" => random.Next(25, 40),
                "medium" => random.Next(15, 25),
                _ => random.Next(8, 15)
            };

            for (int i = 0; i < expenseCount; i++)
            {
                var category = GetRandomCategory(categories, TransactionType.Expense);
                var amount = GetRandomAmount(category.Name, spendingProfile);
                var description = GetTransactionDescription(category.Name);

                var transactionDate = new DateTime(monthDate.Year, monthDate.Month, random.Next(1, daysInMonth));

                transactions.Add(CreateTransaction(
                    user.ID, TransactionType.Expense, amount, description, category.Name,
                    GetRandomPaymentMethod(), transactionDate));
            }
        }

        await DB.SaveAsync(transactions);
        Console.WriteLine($"Criadas {transactions.Count} transações para {user.PersonalInfo.Name}");
    }

    private async Task CreateSampleBudgetsAsync(User user, List<Category> categories)
    {
        var budgets = new List<Budget>();
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Orçamentos para categorias principais
        var budgetCategories = categories
            .Where(c => c.Type == TransactionType.Expense)
            .Take(6) // Apenas 6 categorias com orçamento
            .ToList();

        foreach (var category in budgetCategories)
        {
            var budgetAmount = category.Name switch
            {
                "Moradia" => 1500m,
                "Alimentação" => 800m,
                "Transporte" => 700m,
                "Lazer" => 300m,
                "Saúde" => 200m,
                "Educação" => 250m,
                _ => 150m
            };

            // Ajustar baseado no perfil do usuário
            if (user.Email == "maria.santos@email.com") budgetAmount *= 0.7m;
            if (user.Email == "mauridf@gmail.com") budgetAmount *= 1.3m;

            budgets.Add(new Budget
            {
                UserId = user.ID,
                CategoryId = category.ID,
                Amount = budgetAmount,
                Period = BudgetPeriod.Monthly,
                StartDate = startDate,
                EndDate = endDate,
                Alerts = new AlertSettings { Enabled = true, Threshold = 80 }
            });
        }

        await DB.SaveAsync(budgets);
        Console.WriteLine($"Criados {budgets.Count} orçamentos para {user.PersonalInfo.Name}");
    }

    private async Task CreateSampleGoalsAsync(User user)
    {
        var goals = new List<Goal>();
        var now = DateTime.UtcNow;

        var userGoals = user.FinancialSettings.FinancialGoals;
        var random = new Random();

        foreach (var goalName in userGoals)
        {
            var targetAmount = goalName.ToLower() switch
            {
                var g when g.Contains("carro") => 60000m,
                var g when g.Contains("casa") => 80000m,
                var g when g.Contains("emergência") => 15000m,
                var g when g.Contains("apartamento") => 80000m,
                var g when g.Contains("graduação") => 12000m,
                var g when g.Contains("viagem") => 5000m,
                _ => 10000m
            };

            var monthsToTarget = random.Next(6, 24);
            var currentAmount = targetAmount * (decimal)(random.NextDouble() * 0.6); // 0-60% completo

            goals.Add(new Goal
            {
                UserId = user.ID,
                Name = goalName,
                TargetAmount = targetAmount,
                CurrentAmount = currentAmount,
                TargetDate = now.AddMonths(monthsToTarget),
                Type = GetGoalType(goalName),
                Priority = (PriorityLevel)random.Next(1, 4)
            });
        }

        await DB.SaveAsync(goals);
        Console.WriteLine($"Criadas {goals.Count} metas para {user.PersonalInfo.Name}");
    }

    private async Task CreatePersonalCategoriesAsync(User user)
    {
        var personalCategories = new List<Category>
        {
            new Category
            {
                UserId = user.ID,
                Name = "Investimentos",
                Type = TransactionType.Expense,
                Color = "#059669",
                Icon = "📊"
            },
            new Category
            {
                UserId = user.ID,
                Name = "Doações",
                Type = TransactionType.Expense,
                Color = "#DC2626",
                Icon = "❤️"
            }
        };

        await DB.SaveAsync(personalCategories);
        Console.WriteLine($"Criadas {personalCategories.Count} categorias personalizadas para {user.PersonalInfo.Name}");
    }

    // Métodos auxiliares
    private Transaction CreateTransaction(string userId, TransactionType type, decimal amount,
        string description, string category, string paymentMethod, DateTime date)
    {
        return new Transaction
        {
            UserId = userId,
            Type = type,
            Amount = amount,
            Description = description,
            Category = category,
            PaymentMethod = paymentMethod,
            Date = date,
            CreatedAt = date,
            UpdatedAt = date
        };
    }

    private Category GetRandomCategory(List<Category> categories, TransactionType type)
    {
        var filtered = categories.Where(c => c.Type == type).ToList();
        return filtered[new Random().Next(filtered.Count)];
    }

    private decimal GetRandomAmount(string category, string spendingProfile)
    {
        var baseAmount = category switch
        {
            "Moradia" => 1200m,
            "Alimentação" => 80m,
            "Transporte" => 40m,
            "Lazer" => 120m,
            "Saúde" => 150m,
            "Educação" => 200m,
            "Vestuário" => 100m,
            "Telefonia/Internet" => 120m,
            "Assinaturas" => 50m,
            _ => 60m
        };

        // Variação de ±50%
        var variation = (decimal)(new Random().NextDouble() * 1.0 - 0.5);
        var amount = baseAmount * (1 + variation);

        // Ajustar pelo perfil de gastos
        return spendingProfile switch
        {
            "high" => amount * 1.3m,
            "low" => amount * 0.7m,
            _ => amount
        };
    }

    private string GetTransactionDescription(string category)
    {
        var descriptions = new Dictionary<string, List<string>>
        {
            ["Moradia"] = new() { "Aluguel", "Condomínio", "Conta de luz", "Conta de água", "IPTU" },
            ["Alimentação"] = new() { "Supermercado", "Restaurante", "Delivery", "Padaria", "Feira" },
            ["Transporte"] = new() { "Combustível", "Ônibus", "Metrô", "Uber", "Estacionamento" },
            ["Lazer"] = new() { "Cinema", "Netflix", "Parque", "Show", "Bar com amigos" },
            ["Saúde"] = new() { "Consulta médica", "Farmácia", "Academia", "Plano de saúde" },
            ["Educação"] = new() { "Livros", "Curso online", "Material escolar", "Faculdade" },
            ["Vestuário"] = new() { "Camiseta", "Calça jeans", "Tênis", "Roupas de trabalho" },
            ["Telefonia/Internet"] = new() { "Fatura celular", "Internet fibra", "Streaming" }
        };

        return descriptions.ContainsKey(category)
            ? descriptions[category][new Random().Next(descriptions[category].Count)]
            : $"{category} - {DateTime.UtcNow:MM/yyyy}";
    }

    private string GetRandomPaymentMethod()
    {
        var methods = new[] { "Cartão de Crédito", "Cartão de Débito", "Dinheiro", "PIX", "Transferência" };
        return methods[new Random().Next(methods.Length)];
    }

    private GoalType GetGoalType(string goalName)
    {
        return goalName.ToLower() switch
        {
            var g when g.Contains("carro") || g.Contains("apartamento") => GoalType.Purchase,
            var g when g.Contains("viagem") || g.Contains("europa") => GoalType.Savings,
            var g when g.Contains("emergência") => GoalType.Savings,
            var g when g.Contains("graduação") => GoalType.Education,
            _ => GoalType.Savings
        };
    }

    public async Task ClearAndSeedDataAsync()
    {
        Console.WriteLine("Limpando dados existentes...");

        // Limpar todas as coleções (cuidado: isso apaga tudo!)
        await DB.Database("FinMind").Client.DropDatabaseAsync("FinMind");

        Console.WriteLine("Dados antigos removidos");

        // Executar seed novo
        await SeedDataAsync();
    }
}