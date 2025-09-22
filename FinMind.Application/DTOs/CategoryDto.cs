using FinMind.Domain.Enums;

namespace FinMind.Application.DTOs;

public class CategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? ParentCategoryId { get; set; }
    public decimal? BudgetLimit { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Color { get; set; } = "#000000";
    public string Icon { get; set; } = "📁";
    public string? ParentCategoryId { get; set; }
    public decimal? BudgetLimit { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public decimal? BudgetLimit { get; set; }
}