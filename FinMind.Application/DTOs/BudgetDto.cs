using FinMind.Domain.Enums;
using FinMind.Domain.ValueObjects;

namespace FinMind.Application.DTOs;

public class BudgetDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AlertSettings Alerts { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBudgetDto
{
    public string CategoryId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public AlertSettings Alerts { get; set; } = new();
}

public class UpdateBudgetDto
{
    public decimal Amount { get; set; }
    public AlertSettings Alerts { get; set; } = new();
}