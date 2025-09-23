using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Application.Interfaces.Services;
using FinMind.Domain.Entities;

namespace FinMind.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IJwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Validar confirmação de senha
        if (registerDto.Password != registerDto.ConfirmPassword)
            throw new ArgumentException("Senha e confirmação de senha não coincidem");

        // Verificar se email já existe
        if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
            throw new InvalidOperationException("Email já está em uso");

        // Criar hash da senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Criar usuário
        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            PersonalInfo = new Domain.ValueObjects.PersonalInfo
            {
                Name = registerDto.Name,
                CPF = registerDto.CPF,
                Phone = registerDto.Phone
            }
        };

        var createdUser = await _userRepository.AddAsync(user);

        // Gerar token JWT
        var token = GenerateJwtToken(createdUser);
        var refreshToken = GenerateRefreshToken();

        // Salvar refresh token no usuário
        createdUser.SetRefreshToken(refreshToken, _jwtSettings.RefreshTokenExpirationDays);
        await _userRepository.UpdateAsync(createdUser);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User = MapToUserDto(createdUser)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuário por email
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        // Verificar senha
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        // Atualizar último login
        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);

        // Gerar tokens
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Salvar refresh token
        user.SetRefreshToken(refreshToken, _jwtSettings.RefreshTokenExpirationDays);
        await _userRepository.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new SecurityTokenException("Token inválido");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive || !user.IsRefreshTokenValid(refreshTokenDto.RefreshToken))
            throw new SecurityTokenException("Refresh token inválido");

        // Gerar novos tokens
        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, _jwtSettings.RefreshTokenExpirationDays);
        await _userRepository.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User = MapToUserDto(user)
        };
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.ClearRefreshToken();
            await _userRepository.UpdateAsync(user);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.PersonalInfo.Name)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Token inválido");

        return principal;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.ID,
            Email = user.Email,
            Name = user.PersonalInfo.Name,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin
        };
    }
}