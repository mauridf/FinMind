using MongoDB.Entities;
using FinMind.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace FinMind.Domain.Entities;

[Collection("users")]
public class User : Entity
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public PersonalInfo PersonalInfo { get; set; } = new();
    public FinancialSettings FinancialSettings { get; set; } = new();
    public UserPreferences Preferences { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }

    // Para soft delete
    public bool IsActive { get; set; } = true;
}