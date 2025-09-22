using FinMind.Domain.Enums;

namespace FinMind.Application.DTOs;

public class GoalDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public GoalType Type { get; set; }
    public PriorityLevel Priority { get; set; }
    public decimal Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsNearDueDate { get; set; }
    public int DaysRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateGoalDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public GoalType Type { get; set; } = GoalType.Savings;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
}

public class UpdateGoalDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public PriorityLevel Priority { get; set; }
}

public class UpdateGoalProgressDto
{
    public decimal Amount { get; set; }
}