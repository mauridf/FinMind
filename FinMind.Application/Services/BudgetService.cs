using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Domain.Entities;

namespace FinMind.Application.Services;

public class BudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;

    public BudgetService(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<BudgetDto> GetBudgetByIdAsync(string id)
    {
        var budget = await _budgetRepository.GetByIdAsync(id);
        if (budget == null) throw new ArgumentException("Orçamento não encontrado");

        return await MapToDtoAsync(budget);
    }

    public async Task<List<BudgetDto>> GetUserBudgetsAsync(string userId)
    {
        var budgets = await _budgetRepository.GetByUserIdAsync(userId);
        var budgetDtos = new List<BudgetDto>();

        foreach (var budget in budgets)
        {
            budgetDtos.Add(await MapToDtoAsync(budget));
        }

        return budgetDtos;
    }

    public async Task<List<BudgetDto>> GetActiveBudgetsAsync(string userId)
    {
        var budgets = await _budgetRepository.GetActiveBudgetsAsync(userId);
        var budgetDtos = new List<BudgetDto>();

        foreach (var budget in budgets)
        {
            budgetDtos.Add(await MapToDtoAsync(budget));
        }

        return budgetDtos;
    }

    public async Task<BudgetDto> CreateBudgetAsync(string userId, CreateBudgetDto createBudgetDto)
    {
        // Verificar se a categoria existe
        var category = await _categoryRepository.GetByIdAsync(createBudgetDto.CategoryId);
        if (category == null) throw new ArgumentException("Categoria não encontrada");

        // Verificar se já existe orçamento para esta categoria
        var existingBudget = await _budgetRepository.GetByCategoryAsync(userId, createBudgetDto.CategoryId);
        if (existingBudget != null)
            throw new InvalidOperationException("Já existe um orçamento para esta categoria");

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = createBudgetDto.CategoryId,
            Amount = createBudgetDto.Amount,
            Period = createBudgetDto.Period,
            StartDate = createBudgetDto.StartDate,
            EndDate = createBudgetDto.EndDate,
            Alerts = createBudgetDto.Alerts
        };

        var createdBudget = await _budgetRepository.AddAsync(budget);
        return await MapToDtoAsync(createdBudget);
    }

    public async Task<BudgetDto> UpdateBudgetAsync(string id, UpdateBudgetDto updateBudgetDto)
    {
        var budget = await _budgetRepository.GetByIdAsync(id);
        if (budget == null) throw new ArgumentException("Orçamento não encontrado");

        budget.Amount = updateBudgetDto.Amount;
        budget.Alerts = updateBudgetDto.Alerts;

        await _budgetRepository.UpdateAsync(budget);
        return await MapToDtoAsync(budget);
    }

    public async Task DeleteBudgetAsync(string id)
    {
        var budget = await _budgetRepository.GetByIdAsync(id);
        if (budget == null) throw new ArgumentException("Orçamento não encontrado");

        await _budgetRepository.DeleteAsync(id);
    }

    public async Task<decimal> CalculateBudgetUsageAsync(string budgetId)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId);
        if (budget == null) throw new ArgumentException("Orçamento não encontrado");

        var transactions = await _transactionRepository.GetByUserIdAsync(budget.UserId, budget.StartDate, budget.EndDate);
        var categoryExpenses = transactions
            .Where(t => t.Category == budget.CategoryId && t.Type == Domain.Enums.TransactionType.Expense)
            .Sum(t => t.Amount);

        return budget.Amount > 0 ? (categoryExpenses / budget.Amount) * 100 : 0;
    }

    private async Task<BudgetDto> MapToDtoAsync(Budget budget)
    {
        var category = await _categoryRepository.GetByIdAsync(budget.CategoryId);

        return new BudgetDto
        {
            Id = budget.ID,
            UserId = budget.UserId,
            CategoryId = budget.CategoryId,
            CategoryName = category?.Name ?? "Categoria não encontrada",
            Amount = budget.Amount,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            Alerts = budget.Alerts,
            CreatedAt = budget.CreatedAt,
            IsActive = budget.IsActive
        };
    }
}