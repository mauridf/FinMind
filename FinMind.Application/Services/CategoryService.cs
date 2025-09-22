using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Domain.Entities;
using FinMind.Domain.Enums;

namespace FinMind.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new ArgumentException("Categoria não encontrada");

        return MapToDto(category);
    }

    public async Task<List<CategoryDto>> GetUserCategoriesAsync(string userId)
    {
        var categories = await _categoryRepository.GetByUserIdAsync(userId);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<CategoryDto>> GetUserCategoriesByTypeAsync(string userId, TransactionType type)
    {
        var categories = await _categoryRepository.GetByTypeAsync(userId, type);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateCategoryAsync(string userId, CreateCategoryDto createCategoryDto)
    {
        // Verificar se já existe categoria com mesmo nome para este usuário
        if (await _categoryRepository.ExistsByNameAsync(userId, createCategoryDto.Name))
            throw new InvalidOperationException("Já existe uma categoria com este nome");

        var category = new Category
        {
            UserId = userId,
            Name = createCategoryDto.Name,
            Type = createCategoryDto.Type,
            Color = createCategoryDto.Color,
            Icon = createCategoryDto.Icon,
            ParentCategoryId = createCategoryDto.ParentCategoryId,
            BudgetLimit = createCategoryDto.BudgetLimit
        };

        var createdCategory = await _categoryRepository.AddAsync(category);
        return MapToDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(string id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new ArgumentException("Categoria não encontrada");

        category.Name = updateCategoryDto.Name;
        category.Color = updateCategoryDto.Color;
        category.Icon = updateCategoryDto.Icon;
        category.BudgetLimit = updateCategoryDto.BudgetLimit;

        await _categoryRepository.UpdateAsync(category);
        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new ArgumentException("Categoria não encontrada");

        if (category.IsDefault)
            throw new InvalidOperationException("Não é possível excluir categorias padrão do sistema");

        await _categoryRepository.DeleteAsync(id);
    }

    public async Task<List<CategoryDto>> GetDefaultCategoriesAsync()
    {
        var categories = await _categoryRepository.GetDefaultCategoriesAsync();
        return categories.Select(MapToDto).ToList();
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.ID,
            UserId = category.UserId,
            Name = category.Name,
            Type = category.Type,
            Color = category.Color,
            Icon = category.Icon,
            ParentCategoryId = category.ParentCategoryId,
            BudgetLimit = category.BudgetLimit,
            IsDefault = category.IsDefault,
            CreatedAt = category.CreatedAt
        };
    }
}