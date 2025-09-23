using Moq;
using FinMind.Application.Interfaces.Repositories;
using FinMind.Application.Services;
using FinMind.Domain.Entities;
using FinMind.Application.DTOs;
using FinMind.Infrastructure.Data;
using Xunit;

namespace FinMind.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtSettings = new JwtSettings
        {
            Secret = "TestSecretKeyWithAtLeast32Characters!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        _authService = new AuthService(_userRepositoryMock.Object, _jwtSettings);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValidData()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Name = "Test User",
            CPF = "123.456.789-00",
            Phone = "(11) 99999-9999"
        };

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(registerDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal(registerDto.Email, result.User.Email);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(registerDto.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenPasswordsDontMatch()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "differentpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };
        var user = TestFixtures.CreateTestUser();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal(user.Email, result.User.Email);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenInvalidCredentials()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "wrong@example.com", Password = "wrongpassword" };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginDto));
    }
}