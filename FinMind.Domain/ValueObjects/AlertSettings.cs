namespace FinMind.Domain.ValueObjects;

public class AlertSettings
{
    public bool Enabled { get; set; } = true;
    public decimal Threshold { get; set; } = 80; // 80%
    public bool Notified { get; set; } = false;
}