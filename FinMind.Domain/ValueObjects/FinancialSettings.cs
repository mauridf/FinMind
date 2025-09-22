namespace FinMind.Domain.ValueObjects;

public class FinancialSettings
{
    public string DefaultCurrency { get; set; } = "BRL";
    public decimal MonthlyIncome { get; set; }
    public List<string> FinancialGoals { get; set; } = new();
}