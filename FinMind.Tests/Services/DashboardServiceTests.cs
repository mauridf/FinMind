using Moq;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Application.Services;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using Xunit;

namespace FinMind.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
    private readonly Mock<IGoalRepository> _goalRepositoryMock;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _budgetRepositoryMock = new Mock<IBudgetRepository>();
        _goalRepositoryMock = new Mock<IGoalRepository>();

        _dashboardService = new DashboardService(
            _transactionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _budgetRepositoryMock.Object,
            _goalRepositoryMock.Object);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldCalculateCorrectTotals()
    {
        // Arrange
        var userId = "user123";
        var monthlyIncome = 5000m;
        var monthlyExpenses = 3500m;
        var totalIncome = 15000m;
        var totalExpenses = 12000m;

        _transactionRepositoryMock.Setup(x => x.GetTotalAmountByTypeAsync(
            userId, TransactionType.Income, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(monthlyIncome);
        _transactionRepositoryMock.Setup(x => x.GetTotalAmountByTypeAsync(
            userId, TransactionType.Expense, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(monthlyExpenses);
        _transactionRepositoryMock.Setup(x => x.GetTotalAmountByTypeAsync(userId, TransactionType.Income, null, null))
            .ReturnsAsync(totalIncome);
        _transactionRepositoryMock.Setup(x => x.GetTotalAmountByTypeAsync(userId, TransactionType.Expense, null, null))
            .ReturnsAsync(totalExpenses);

        _budgetRepositoryMock.Setup(x => x.GetActiveBudgetsAsync(userId))
            .ReturnsAsync(new List<Budget>());
        _goalRepositoryMock.Setup(x => x.GetActiveGoalsAsync(userId))
            .ReturnsAsync(new List<Goal>());
        _goalRepositoryMock.Setup(x => x.GetCompletedGoalsAsync(userId))
            .ReturnsAsync(new List<Goal>());
        //_transactionRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
        //    .ReturnsAsync(new List<Transaction>());

        // Act
        var result = await _dashboardService.GetDashboardSummaryAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalIncome - totalExpenses, result.TotalBalance);
        Assert.Equal(monthlyIncome, result.TotalIncome);
        Assert.Equal(monthlyExpenses, result.TotalExpenses);
    }

    [Fact]
    public async Task GetSpendingByCategoryAsync_ShouldReturnCorrectPercentages()
    {
        // Arrange
        var userId = "user123";
        var transactions = new List<Transaction>
        {
            new Transaction { UserId = userId, Type = TransactionType.Expense, Amount = 300m, Category = "Food" },
            new Transaction { UserId = userId, Type = TransactionType.Expense, Amount = 200m, Category = "Transport" },
            new Transaction { UserId = userId, Type = TransactionType.Expense, Amount = 500m, Category = "Rent" }
        };

        var categories = new List<Category>
        {
            new Category { Name = "Food", Color = "#FF0000", Icon = "🍕" },
            new Category { Name = "Transport", Color = "#00FF00", Icon = "🚗" },
            new Category { Name = "Rent", Color = "#0000FF", Icon = "🏠" }
        };

        _transactionRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, null, null))
            .ReturnsAsync(transactions);
        _categoryRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(categories);

        // Act
        var result = await _dashboardService.GetSpendingByCategoryAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        var total = 300m + 200m + 500m; // 1000m
        var foodCategory = result.First(x => x.Category == "Food");
        Assert.Equal(30m, foodCategory.Percentage); // 300 / 1000 * 100 = 30%

        var rentCategory = result.First(x => x.Category == "Rent");
        Assert.Equal(50m, rentCategory.Percentage); // 500 / 1000 * 100 = 50%
    }

    [Fact]
    public async Task GetGoalsProgressAsync_ShouldCalculateCorrectProgress()
    {
        // Arrange
        var userId = "user123";
        var goals = new List<Goal>
        {
            new Goal
            {
                UserId = userId,
                Name = "Test Goal",
                TargetAmount = 1000m,
                CurrentAmount = 500m,
                TargetDate = DateTime.UtcNow.AddDays(100),
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            }
        };

        _goalRepositoryMock.Setup(x => x.GetActiveGoalsAsync(userId))
            .ReturnsAsync(goals);

        // Act
        var result = await _dashboardService.GetGoalsProgressAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var goalProgress = result.First();
        Assert.Equal(50m, goalProgress.ProgressPercentage); // 500 / 1000 * 100 = 50%
        Assert.True(goalProgress.DaysRemaining > 0);
    }
}