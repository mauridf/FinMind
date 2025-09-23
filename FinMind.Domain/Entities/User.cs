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

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public PersonalInfo PersonalInfo { get; set; } = new();
    public FinancialSettings FinancialSettings { get; set; } = new();
    public UserPreferences Preferences { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; } = true;

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, int expiryDays)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsRefreshTokenValid(string refreshToken)
    {
        return RefreshToken == refreshToken &&
               RefreshTokenExpiry.HasValue &&
               RefreshTokenExpiry.Value > DateTime.UtcNow;
    }
}