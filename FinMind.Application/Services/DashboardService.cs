using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Domain.Enums;

namespace FinMind.Application.Services;

public class DashboardService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IGoalRepository _goalRepository;

    public DashboardService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IBudgetRepository budgetRepository,
        IGoalRepository goalRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _budgetRepository = budgetRepository;
        _goalRepository = goalRepository;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Calcular totais do mês atual
        var monthlyIncome = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Income, startOfMonth, endOfMonth);
        var monthlyExpenses = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Expense, startOfMonth, endOfMonth);

        // Calcular saldo total (todas as transações)
        var totalIncome = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Income);
        var totalExpenses = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Expense);
        var totalBalance = totalIncome - totalExpenses;

        // Calcular orçamento mensal
        var activeBudgets = await _budgetRepository.GetActiveBudgetsAsync(userId);
        var monthlyBudget = activeBudgets.Sum(b => b.Amount);

        // Buscar metas
        var activeGoals = await _goalRepository.GetActiveGoalsAsync(userId);
        var completedGoals = await _goalRepository.GetCompletedGoalsAsync(userId);

        // Buscar total de transações
        var allTransactions = await _transactionRepository.GetByUserIdAsync(userId);

        // Calcular uso do orçamento
        var budgetUsagePercentage = monthlyBudget > 0 ? (monthlyExpenses / monthlyBudget) * 100 : 0;

        return new DashboardSummaryDto
        {
            TotalBalance = totalBalance,
            TotalIncome = monthlyIncome,
            TotalExpenses = monthlyExpenses,
            MonthlyBudget = monthlyBudget,
            BudgetUsagePercentage = Math.Round(budgetUsagePercentage, 2),
            TotalTransactions = allTransactions.Count,
            ActiveGoals = activeGoals.Count,
            CompletedGoals = completedGoals.Count
        };
    }

    public async Task<List<CategorySpendingDto>> GetSpendingByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(userId, startDate, endDate);
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
        var categories = await _categoryRepository.GetByUserIdAsync(userId);

        var totalExpenses = expenseTransactions.Sum(t => t.Amount);
        var spendingByCategory = expenseTransactions
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpenses > 0 ? (g.Sum(t => t.Amount) / totalExpenses) * 100 : 0
            })
            .ToList();

        var result = new List<CategorySpendingDto>();

        foreach (var item in spendingByCategory)
        {
            var category = categories.FirstOrDefault(c => c.Name == item.Category);
            result.Add(new CategorySpendingDto
            {
                Category = item.Category,
                Amount = item.Amount,
                Percentage = Math.Round(item.Percentage, 2),
                Color = category?.Color ?? "#6B7280",
                Icon = category?.Icon ?? "📦"
            });
        }

        return result.OrderByDescending(x => x.Amount).ToList();
    }

    public async Task<List<MonthlySummaryDto>> GetMonthlySummaryAsync(string userId, int monthsBack = 6)
    {
        var result = new List<MonthlySummaryDto>();
        var now = DateTime.UtcNow;

        for (int i = monthsBack - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var income = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Income, startOfMonth, endOfMonth);
            var expenses = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Expense, startOfMonth, endOfMonth);

            result.Add(new MonthlySummaryDto
            {
                Year = date.Year,
                Month = date.Month,
                Income = income,
                Expenses = expenses,
                Balance = income - expenses
            });
        }

        return result;
    }

    public async Task<List<CashFlowProjectionDto>> GetCashFlowProjectionAsync(string userId, int daysAhead = 30)
    {
        var projection = new List<CashFlowProjectionDto>();
        var now = DateTime.UtcNow;
        var currentBalance = await GetCurrentBalanceAsync(userId);

        // Média de receitas e despesas dos últimos 90 dias
        var avgIncome = await GetAverageDailyAmountAsync(userId, TransactionType.Income, 90);
        var avgExpenses = await GetAverageDailyAmountAsync(userId, TransactionType.Expense, 90);

        decimal runningBalance = currentBalance;

        for (int i = 1; i <= daysAhead; i++)
        {
            var date = now.AddDays(i);
            var dailyIncome = avgIncome;
            var dailyExpenses = avgExpenses;

            // Ajustar para finais de semana (menos gastos em dias de semana?)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                dailyExpenses *= 1.2m; // 20% mais gastos no fim de semana
            }

            runningBalance += dailyIncome - dailyExpenses;

            projection.Add(new CashFlowProjectionDto
            {
                Date = date,
                ProjectedBalance = Math.Round(runningBalance, 2),
                ExpectedIncome = Math.Round(dailyIncome, 2),
                ExpectedExpenses = Math.Round(dailyExpenses, 2)
            });
        }

        return projection;
    }

    public async Task<List<GoalProgressDto>> GetGoalsProgressAsync(string userId)
    {
        var goals = await _goalRepository.GetActiveGoalsAsync(userId);
        var result = new List<GoalProgressDto>();

        foreach (var goal in goals)
        {
            var timeSpanRemaining = goal.TargetDate - DateTime.UtcNow;
            var timeSpanSinceCreation = DateTime.UtcNow - goal.CreatedAt;

            // Usar métodos mais seguros para evitar divisão por zero
            var daysRemaining = Math.Max(1, (int)timeSpanRemaining.TotalDays);
            var daysSinceCreation = Math.Max(1, (int)timeSpanSinceCreation.TotalDays);

            // Usar decimal para todos os cálculos
            var dailyTarget = goal.TargetAmount / daysRemaining;
            var currentDaily = goal.CurrentAmount / daysSinceCreation;

            var isOnTrack = currentDaily >= dailyTarget * 0.8m;

            result.Add(new GoalProgressDto
            {
                GoalName = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                ProgressPercentage = Math.Round(goal.Progress, 2),
                DaysRemaining = daysRemaining,
                IsOnTrack = isOnTrack
            });
        }

        return result.OrderByDescending(g => g.ProgressPercentage).ToList();
    }

    public async Task<List<BudgetStatusDto>> GetBudgetStatusAsync(string userId)
    {
        var activeBudgets = await _budgetRepository.GetActiveBudgetsAsync(userId);
        var result = new List<BudgetStatusDto>();
        var now = DateTime.UtcNow;

        foreach (var budget in activeBudgets)
        {
            var spentAmount = await _transactionRepository.GetTotalAmountByTypeAsync(
                userId, TransactionType.Expense, budget.StartDate, budget.EndDate);

            var usagePercentage = budget.Amount > 0 ? (spentAmount / budget.Amount) * 100 : 0;
            var isOverBudget = usagePercentage > 100;
            var isNearLimit = usagePercentage >= budget.Alerts.Threshold && !isOverBudget;

            result.Add(new BudgetStatusDto
            {
                Category = (await _categoryRepository.GetByIdAsync(budget.CategoryId))?.Name ?? "Categoria não encontrada",
                BudgetAmount = budget.Amount,
                SpentAmount = spentAmount,
                RemainingAmount = Math.Max(0, budget.Amount - spentAmount),
                UsagePercentage = Math.Round(usagePercentage, 2),
                IsOverBudget = isOverBudget,
                IsNearLimit = isNearLimit
            });
        }

        return result.OrderByDescending(b => b.UsagePercentage).ToList();
    }

    public async Task<Dictionary<string, decimal>> GetFinancialHealthMetricsAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var monthlyIncome = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Income, startOfMonth, endOfMonth);
        var monthlyExpenses = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Expense, startOfMonth, endOfMonth);
        var totalBalance = await GetCurrentBalanceAsync(userId);

        var savingsRate = monthlyIncome > 0 ? ((monthlyIncome - monthlyExpenses) / monthlyIncome) * 100 : 0;
        var emergencyFundMonths = monthlyExpenses > 0 ? totalBalance / monthlyExpenses : 0;

        return new Dictionary<string, decimal>
        {
            ["SavingsRate"] = Math.Round(savingsRate, 2),
            ["EmergencyFundMonths"] = Math.Round(emergencyFundMonths, 2),
            ["MonthlyExpenseToIncomeRatio"] = monthlyIncome > 0 ? Math.Round((monthlyExpenses / monthlyIncome) * 100, 2) : 0,
            ["NetWorth"] = totalBalance
        };
    }

    private async Task<decimal> GetCurrentBalanceAsync(string userId)
    {
        var totalIncome = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Income);
        var totalExpenses = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Expense);
        return totalIncome - totalExpenses;
    }

    private async Task<decimal> GetAverageDailyAmountAsync(string userId, TransactionType type, int daysBack)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-daysBack);

        var totalAmount = await _transactionRepository.GetTotalAmountByTypeAsync(userId, type, startDate, endDate);
        return totalAmount / daysBack;
    }
}