using MongoDB.Entities;
using FinMind.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinMind.Domain.Entities;

[Collection("goals")]
public class Goal : Entity
{
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; } = 0;
    public DateTime TargetDate { get; set; }
    public GoalType Type { get; set; } = GoalType.Savings;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    // Propriedade calculada - progresso em porcentagem
    public decimal Progress
    {
        get
        {
            if (TargetAmount <= 0) return 0;
            return (CurrentAmount / TargetAmount) * 100;
        }
    }

    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Método para verificar se a meta está próxima do vencimento
    public bool IsNearDueDate(int daysThreshold = 30)
    {
        var daysRemaining = (TargetDate - DateTime.UtcNow).TotalDays;
        return daysRemaining <= daysThreshold && daysRemaining > 0;
    }
}