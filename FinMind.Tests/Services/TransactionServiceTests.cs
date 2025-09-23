using Moq;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Application.Services;
using FinMind.Application.DTOs;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;
using Xunit;

namespace FinMind.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _transactionService = new TransactionService(_transactionRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldCreateTransaction_WhenValidData()
    {
        // Arrange
        var userId = "user123";
        var createDto = new CreateTransactionDto
        {
            Type = TransactionType.Expense,
            Amount = 100.50m,
            Description = "Test Transaction",
            Category = "Food",
            PaymentMethod = "Credit Card",
            Date = DateTime.UtcNow
        };

        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        // Act
        var result = await _transactionService.CreateTransactionAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Amount, result.Amount);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(userId, result.UserId);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task GetUserBalanceAsync_ShouldCalculateCorrectBalance()
    {
        // Arrange
        var userId = "user123";
        var totalIncome = 5000m;
        var totalExpenses = 3500m;

        _transactionRepositoryMock.Setup(x => x.GetTotalAmountByTypeAsync(userId, TransactionType.Income, null, null))
            .ReturnsAsync(totalIncome);
        _transactionRepositoryMock.Setup(x => x.GetTotalAmountByTypeAsync(userId, TransactionType.Expense, null, null))
            .ReturnsAsync(totalExpenses);

        // Act
        var balance = await _transactionService.GetUserBalanceAsync(userId);

        // Assert
        Assert.Equal(totalIncome - totalExpenses, balance);
    }

    [Fact]
    public async Task GetSpendingByCategoryAsync_ShouldGroupTransactionsCorrectly()
    {
        // Arrange
        var userId = "user123";
        var transactions = new List<Transaction>
        {
            new Transaction { UserId = userId, Type = TransactionType.Expense, Amount = 100m, Category = "Food" },
            new Transaction { UserId = userId, Type = TransactionType.Expense, Amount = 200m, Category = "Food" },
            new Transaction { UserId = userId, Type = TransactionType.Expense, Amount = 150m, Category = "Transport" }
        };

        _transactionRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, null, null))
            .ReturnsAsync(transactions);

        // Act
        var result = await _transactionService.GetSpendingByCategoryAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Food e Transport

        var foodCategory = result.FirstOrDefault(x => x.Key == "Food");
        Assert.NotNull(foodCategory);
        Assert.Equal(300m, foodCategory.Value); // 100 + 200
    }

    [Fact]
    public async Task UpdateTransactionAsync_ShouldUpdateTransaction_WhenExists()
    {
        // Arrange
        var transactionId = "trans123";
        var existingTransaction = TestFixtures.CreateTestTransaction("user123");
        var updateDto = new UpdateTransactionDto
        {
            Description = "Updated Description",
            Category = "Updated Category",
            Amount = 200m
        };

        _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId))
            .ReturnsAsync(existingTransaction);
        _transactionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.UpdateTransactionAsync(transactionId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Equal(updateDto.Amount, result.Amount);
        _transactionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTransactionAsync_ShouldThrowException_WhenTransactionNotFound()
    {
        // Arrange
        var transactionId = "nonexistent";
        var updateDto = new UpdateTransactionDto();

        _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId))
            .ReturnsAsync((Transaction?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _transactionService.UpdateTransactionAsync(transactionId, updateDto));
    }
}