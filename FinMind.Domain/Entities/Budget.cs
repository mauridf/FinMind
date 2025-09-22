using MongoDB.Entities;
using FinMind.Domain.Enums;
using FinMind.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace FinMind.Domain.Entities;

[Collection("budgets")]
public class Budget : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public AlertSettings Alerts { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Método para verificar se o orçamento está ativo
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

    // Método para calcular o progresso do orçamento (será implementado depois)
    public decimal CalculateProgress(decimal currentSpending)
    {
        if (Amount <= 0) return 0;
        return (currentSpending / Amount) * 100;
    }
}