using FinMind.Domain.Enums;

namespace FinMind.Application.DTOs;

public class DashboardSummaryDto
{
    public decimal TotalBalance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal MonthlyBudget { get; set; }
    public decimal BudgetUsagePercentage { get; set; }
    public int TotalTransactions { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedGoals { get; set; }
}

public class CategorySpendingDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Balance { get; set; }
}

public class CashFlowProjectionDto
{
    public DateTime Date { get; set; }
    public decimal ProjectedBalance { get; set; }
    public decimal ExpectedIncome { get; set; }
    public decimal ExpectedExpenses { get; set; }
}

public class GoalProgressDto
{
    public string GoalName { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsOnTrack { get; set; }
}

public class BudgetStatusDto
{
    public string Category { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal UsagePercentage { get; set; }
    public bool IsOverBudget { get; set; }
    public bool IsNearLimit { get; set; }
}