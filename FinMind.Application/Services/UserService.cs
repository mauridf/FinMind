using FinMind.Application.DTOs;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Domain.Entities;

namespace FinMind.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || !user.IsActive)
            throw new ArgumentException("Usuário não encontrado");

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Verificar se email já existe
        if (await _userRepository.ExistsByEmailAsync(createUserDto.Email))
            throw new InvalidOperationException("Email já está em uso");

        // Criar hash da senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = new User
        {
            Email = createUserDto.Email,
            PasswordHash = passwordHash,
            PersonalInfo = new Domain.ValueObjects.PersonalInfo
            {
                Name = createUserDto.Name,
                CPF = createUserDto.CPF,
                Phone = createUserDto.Phone
            }
        };

        var createdUser = await _userRepository.AddAsync(user);
        return MapToDto(createdUser);
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || !user.IsActive)
            throw new ArgumentException("Usuário não encontrado");

        user.PersonalInfo.Name = updateUserDto.Name;
        user.PersonalInfo.Phone = updateUserDto.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        // Soft delete
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> ValidatePasswordAsync(string userId, string password)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
            throw new ArgumentException("Usuário não encontrado");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Senha atual incorreta");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    private static UserDto MapToDto(User user)
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