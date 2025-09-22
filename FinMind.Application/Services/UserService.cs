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
        if (user == null) throw new ArgumentException("Usuário não encontrado");

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Verificar se email já existe
        if (await _userRepository.ExistsByEmailAsync(createUserDto.Email))
            throw new InvalidOperationException("Email já está em uso");

        // TODO: Adicionar hash de senha depois
        var user = new User
        {
            Email = createUserDto.Email,
            PasswordHash = createUserDto.Password, // Temporário - vamos hash depois
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