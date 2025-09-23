using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using FinMind.Domain.ValueObjects;

namespace FinMind.Tests;

public static class TestFixtures
{
    public static User CreateTestUser()
    {
        return new User
        {
            ID = "user123",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            PersonalInfo = new PersonalInfo
            {
                Name = "Test User",
                CPF = "123.456.789-00",
                Phone = "(11) 99999-9999"
            },
            IsActive = true
        };
    }

    public static Transaction CreateTestTransaction(string userId, TransactionType type = TransactionType.Expense)
    {
        return new Transaction
        {
            UserId = userId,
            Type = type,
            Amount = 100.50m,
            Description = "Test Transaction",
            Category = "Test Category",
            PaymentMethod = "Credit Card",
            Date = DateTime.UtcNow.AddDays(-1)
        };
    }

    public static Category CreateTestCategory(string userId, TransactionType type = TransactionType.Expense)
    {
        return new Category
        {
            UserId = userId,
            Name = "Test Category",
            Type = type,
            Color = "#FF0000",
            Icon = "📁"
        };
    }

    public static Budget CreateTestBudget(string userId, string categoryId)
    {
        return new Budget
        {
            UserId = userId,
            CategoryId = categoryId,
            Amount = 1000m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31)
        };
    }

    public static Goal CreateTestGoal(string userId)
    {
        return new Goal
        {
            UserId = userId,
            Name = "Test Goal",
            TargetAmount = 5000m,
            CurrentAmount = 1000m,
            TargetDate = DateTime.UtcNow.AddMonths(6),
            Type = GoalType.Savings,
            Priority = PriorityLevel.Medium
        };
    }
}