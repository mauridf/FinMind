namespace FinMind.Domain.ValueObjects;

public class UserPreferences
{
    public string Language { get; set; } = "pt-BR";
    public bool Notifications { get; set; } = true;
    public string Theme { get; set; } = "light";
    public string TimeZone { get; set; } = "America/Sao_Paulo";
}