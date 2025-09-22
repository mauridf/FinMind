using MongoDB.Entities;
using FinMind.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinMind.Domain.Entities;

[Collection("categories")]
public class Category : Entity
{
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public TransactionType Type { get; set; }
    public string Color { get; set; } = "#000000"; // Cor em hex
    public string Icon { get; set; } = "📁"; // Ícone padrão
    public string? ParentCategoryId { get; set; } // Para subcategorias
    public decimal? BudgetLimit { get; set; }
    public bool IsDefault { get; set; } = false; // Categorias padrão do sistema
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Método auxiliar para verificar se é uma categoria pai
    public bool IsParentCategory => string.IsNullOrEmpty(ParentCategoryId);
}