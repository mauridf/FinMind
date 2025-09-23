namespace FinMind.Application.Interfaces.Services;

public interface IJwtSettings
{
    string Secret { get; }
    string Issuer { get; }
    string Audience { get; }
    int ExpirationMinutes { get; }
    int RefreshTokenExpirationDays { get; }
}