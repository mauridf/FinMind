using MongoDB.Entities;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using FinMind.Domain.ValueObjects;
using FinMind.Application.Interfaces.Services;

namespace FinMind.Infrastructure.Services;

public class SeedService : ISeedService
{
    public async Task SeedDataAsync()
    {
        // Verificar se já existe dados para não duplicar
        var hasUsers = await DB.Find<User>().ExecuteAnyAsync();
        if (hasUsers) return;

        Console.WriteLine("Iniciando seed de dados...");

        // 1. Criar categorias padrão do sistema
        var defaultCategories = await CreateDefaultCategoriesAsync();

        // 2. Criar usuário de teste
        var testUser = await CreateTestUserAsync();

        // 3. Criar transações de exemplo
        await CreateSampleTransactionsAsync(testUser, defaultCategories);

        // 4. Criar orçamentos de exemplo
        await CreateSampleBudgetsAsync(testUser, defaultCategories);

        // 5. Criar metas de exemplo
        await CreateSampleGoalsAsync(testUser);

        Console.WriteLine("Seed de dados concluído com sucesso!");
    }

    private async Task<List<Category>> CreateDefaultCategoriesAsync()
    {
        var defaultCategories = new List<Category>
        {
            // Receitas
            new Category { Name = "Salário", Type = TransactionType.Income, Color = "#10B981", Icon = "💰", IsDefault = true },
            new Category { Name = "Freelance", Type = TransactionType.Income, Color = "#10B981", Icon = "💼", IsDefault = true },
            new Category { Name = "Investimentos", Type = TransactionType.Income, Color = "#10B981", Icon = "📈", IsDefault = true },
            new Category { Name = "Presente", Type = TransactionType.Income, Color = "#10B981", Icon = "🎁", IsDefault = true },
            new Category { Name = "Outras Receitas", Type = TransactionType.Income, Color = "#10B981", Icon = "📥", IsDefault = true },

            // Despesas
            new Category { Name = "Moradia", Type = TransactionType.Expense, Color = "#EF4444", Icon = "🏠", IsDefault = true },
            new Category { Name = "Alimentação", Type = TransactionType.Expense, Color = "#F59E0B", Icon = "🍕", IsDefault = true },
            new Category { Name = "Transporte", Type = TransactionType.Expense, Color = "#3B82F6", Icon = "🚗", IsDefault = true },
            new Category { Name = "Saúde", Type = TransactionType.Expense, Color = "#8B5CF6", Icon = "🏥", IsDefault = true },
            new Category { Name = "Educação", Type = TransactionType.Expense, Color = "#06B6D4", Icon = "📚", IsDefault = true },
            new Category { Name = "Lazer", Type = TransactionType.Expense, Color = "#EC4899", Icon = "🎬", IsDefault = true },
            new Category { Name = "Vestuário", Type = TransactionType.Expense, Color = "#F97316", Icon = "👕", IsDefault = true },
            new Category { Name = "Telefonia/Internet", Type = TransactionType.Expense, Color = "#6366F1", Icon = "📱", IsDefault = true },
            new Category { Name = "Outras Despesas", Type = TransactionType.Expense, Color = "#6B7280", Icon = "📦", IsDefault = true }
        };

        await DB.SaveAsync(defaultCategories);
        Console.WriteLine($"Criadas {defaultCategories.Count} categorias padrão");
        return defaultCategories;
    }

    private async Task<User> CreateTestUserAsync()
    {
        var testUser = new User
        {
            Email = "teste@finmind.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Senha123!"),
            PersonalInfo = new PersonalInfo
            {
                Name = "Usuário Teste FinMind",
                CPF = "123.456.789-00",
                Phone = "(11) 99999-9999"
            },
            FinancialSettings = new FinancialSettings
            {
                DefaultCurrency = "BRL",
                MonthlyIncome = 5000m,
                FinancialGoals = new List<string> { "Comprar um carro", "Viajar para Europa" }
            },
            Preferences = new UserPreferences
            {
                Language = "pt-BR",
                Notifications = true,
                Theme = "light"
            }
        };

        await testUser.SaveAsync();
        Console.WriteLine("Usuário de teste criado: teste@finmind.com / Senha123!");
        return testUser;
    }

    private async Task CreateSampleTransactionsAsync(User user, List<Category> categories)
    {
        var random = new Random();
        var transactions = new List<Domain.Entities.Transaction>();
        var now = DateTime.UtcNow;

        // Receitas do último mês
        var incomeCategories = categories.Where(c => c.Type == TransactionType.Income).ToList();
        transactions.Add(new Domain.Entities.Transaction
        {
            UserId = user.ID,
            Type = TransactionType.Income,
            Amount = 5000m,
            Description = "Salário mensal",
            Category = "Salário",
            PaymentMethod = "Transferência",
            Date = now.AddDays(-15)
        });

        // Despesas dos últimos 30 dias
        var expenseCategories = categories.Where(c => c.Type == TransactionType.Expense).ToList();

        for (int i = 1; i <= 20; i++)
        {
            var category = expenseCategories[random.Next(expenseCategories.Count)];
            var amount = random.Next(50, 500);

            transactions.Add(new Domain.Entities.Transaction
            {
                UserId = user.ID,
                Type = TransactionType.Expense,
                Amount = amount,
                Description = GetRandomDescription(category.Name),
                Category = category.Name,
                PaymentMethod = GetRandomPaymentMethod(),
                Date = now.AddDays(-random.Next(1, 30)),
                Tags = GetRandomTags(category.Name)
            });
        }

        await DB.SaveAsync(transactions);
        Console.WriteLine($"Criadas {transactions.Count} transações de exemplo");
    }

    private async Task CreateSampleBudgetsAsync(User user, List<Category> categories)
    {
        var expenseCategories = categories.Where(c => c.Type == TransactionType.Expense).Take(5).ToList();
        var budgets = new List<Budget>();
        var now = DateTime.UtcNow;

        foreach (var category in expenseCategories)
        {
            var budgetAmount = category.Name switch
            {
                "Moradia" => 1500m,
                "Alimentação" => 800m,
                "Transporte" => 400m,
                "Lazer" => 300m,
                _ => 200m
            };

            budgets.Add(new Budget
            {
                UserId = user.ID,
                CategoryId = category.ID,
                Amount = budgetAmount,
                Period = BudgetPeriod.Monthly,
                StartDate = new DateTime(now.Year, now.Month, 1),
                EndDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1),
                Alerts = new AlertSettings { Enabled = true, Threshold = 80 }
            });
        }

        await DB.SaveAsync(budgets);
        Console.WriteLine($"Criados {budgets.Count} orçamentos de exemplo");
    }

    private async Task CreateSampleGoalsAsync(User user)
    {
        var goals = new List<Goal>
        {
            new Goal
            {
                UserId = user.ID,
                Name = "Viagem para Europa",
                TargetAmount = 15000m,
                CurrentAmount = 3500m,
                TargetDate = DateTime.UtcNow.AddMonths(18),
                Type = GoalType.Savings,
                Priority = PriorityLevel.High
            },
            new Goal
            {
                UserId = user.ID,
                Name = "Notebook novo",
                TargetAmount = 4000m,
                CurrentAmount = 1200m,
                TargetDate = DateTime.UtcNow.AddMonths(6),
                Type = GoalType.Purchase,
                Priority = PriorityLevel.Medium
            },
            new Goal
            {
                UserId = user.ID,
                Name = "Fundo de emergência",
                TargetAmount = 10000m,
                CurrentAmount = 6500m,
                TargetDate = DateTime.UtcNow.AddMonths(12),
                Type = GoalType.Savings,
                Priority = PriorityLevel.Critical
            }
        };

        await DB.SaveAsync(goals);
        Console.WriteLine($"Criadas {goals.Count} metas de exemplo");
    }

    // Métodos auxiliares para gerar dados randômicos
    private static string GetRandomDescription(string category)
    {
        var descriptions = new Dictionary<string, List<string>>
        {
            ["Moradia"] = new() { "Aluguel", "Condomínio", "Conta de luz", "Conta de água", "Internet" },
            ["Alimentação"] = new() { "Supermercado", "Restaurante", "Delivery", "Padaria", "Feira" },
            ["Transporte"] = new() { "Combustível", "Ônibus", "Metrô", "Uber", "Manutenção do carro" },
            ["Lazer"] = new() { "Cinema", "Streaming", "Parque", "Show", "Viagem" },
            ["Saúde"] = new() { "Consulta médica", "Farmácia", "Academia", "Plano de saúde", "Exames" }
        };

        return descriptions.ContainsKey(category)
            ? descriptions[category][new Random().Next(descriptions[category].Count)]
            : $"{category} - {DateTime.UtcNow:dd/MM}";
    }

    private static string GetRandomPaymentMethod()
    {
        var methods = new[] { "Cartão de Crédito", "Cartão de Débito", "Dinheiro", "PIX", "Transferência" };
        return methods[new Random().Next(methods.Length)];
    }

    private static List<string> GetRandomTags(string category)
    {
        var tags = new Dictionary<string, List<string>>
        {
            ["Moradia"] = new() { "casa", "fixo", "essencial" },
            ["Alimentação"] = new() { "comida", "necessidade", "semanal" },
            ["Lazer"] = new() { "diversão", "opcional", "fim-de-semana" },
            ["Transporte"] = new() { "trabalho", "deslocamento", "necessidade" }
        };

        return tags.ContainsKey(category) ? tags[category] : new List<string>();
    }
}