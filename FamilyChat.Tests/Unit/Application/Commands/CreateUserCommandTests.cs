using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyChat.Tests.Unit.Application.Commands;

public class CreateUserCommandTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();
        _handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ShouldCreateUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()));

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        
        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u => 
                u.Email == command.Email &&
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(new User("existing@example.com", "John", "Doe", "Password123!"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command));
    }
} 