using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;

namespace FinMind.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<TransactionDto> GetTransactionByIdAsync(string id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null) throw new ArgumentException("Transação não encontrada");

        return MapToDto(transaction);
    }

    public async Task<List<TransactionDto>> GetUserTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(userId, startDate, endDate);
        return transactions.Select(MapToDto).ToList();
    }

    public async Task<TransactionDto> CreateTransactionAsync(string userId, CreateTransactionDto createTransactionDto)
    {
        // Validar se a categoria existe (opcional - podemos criar categorias dinamicamente depois)
        // var category = await _categoryRepository.GetByIdAsync(createTransactionDto.Category);
        // if (category == null) throw new ArgumentException("Categoria não encontrada");

        var transaction = new Transaction
        {
            UserId = userId,
            Type = createTransactionDto.Type,
            Amount = createTransactionDto.Amount,
            Description = createTransactionDto.Description,
            Category = createTransactionDto.Category,
            PaymentMethod = createTransactionDto.PaymentMethod,
            Date = createTransactionDto.Date
        };

        var createdTransaction = await _transactionRepository.AddAsync(transaction);
        return MapToDto(createdTransaction);
    }

    public async Task<TransactionDto> UpdateTransactionAsync(string id, UpdateTransactionDto updateTransactionDto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null) throw new ArgumentException("Transação não encontrada");

        transaction.Description = updateTransactionDto.Description;
        transaction.Category = updateTransactionDto.Category;
        transaction.Amount = updateTransactionDto.Amount;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _transactionRepository.UpdateAsync(transaction);
        return MapToDto(transaction);
    }

    public async Task DeleteTransactionAsync(string id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null) throw new ArgumentException("Transação não encontrada");

        await _transactionRepository.DeleteAsync(id);
    }

    public async Task<decimal> GetUserBalanceAsync(string userId)
    {
        var totalIncome = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Income);
        var totalExpenses = await _transactionRepository.GetTotalAmountByTypeAsync(userId, TransactionType.Expense);

        return totalIncome - totalExpenses;
    }

    public async Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(userId, startDate, endDate);
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense);

        return expenseTransactions
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.ID,
            UserId = transaction.UserId,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Category = transaction.Category,
            PaymentMethod = transaction.PaymentMethod,
            Date = transaction.Date,
            CreatedAt = transaction.CreatedAt
        };
    }
}