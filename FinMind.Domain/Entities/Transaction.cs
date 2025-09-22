using MongoDB.Entities;
using FinMind.Domain.Enums;
using FinMind.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace FinMind.Domain.Entities;

[Collection("transactions")]
public class Transaction : Entity
{
    public string UserId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsRecurring { get; set; } = false;
    public RecurringFrequency? RecurringFrequency { get; set; }
    public List<string> Tags { get; set; } = new();
    public LocationInfo? Location { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}