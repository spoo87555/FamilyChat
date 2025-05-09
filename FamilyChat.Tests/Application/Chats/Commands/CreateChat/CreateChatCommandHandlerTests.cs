using System;
using System.Threading;
using System.Threading.Tasks;
using FamilyChat.Application.Chats.Commands.CreateChat;
using FamilyChat.Domain.Entities;
using FamilyChat.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FamilyChat.Tests.Application.Chats.Commands.CreateChat;

public class CreateChatCommandHandlerTests : TestBase
{
    private readonly CreateChatCommandHandler _handler;

    public CreateChatCommandHandlerTests()
    {
        var loggerMock = new Mock<ILogger<CreateChatCommandHandler>>();
        _handler = new CreateChatCommandHandler(
            ChatRepositoryMock.Object,
            UserRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateChat()
    {
        // Arrange
        var command = new CreateChatCommand
        {
            Name = "Test Chat",
            Description = "Test Description",
            IsDefault = false,
            CreatedById = Guid.NewGuid()
        };

        var user = CreateTestUser(command.CreatedById);

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(command.CreatedById, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        ChatRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Chat>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chat chat, CancellationToken _) => chat);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.IsDefault, result.IsDefault);
        Assert.Equal(command.CreatedById, result.CreatedById);
        Assert.NotEqual(default, result.Id);
        Assert.NotEqual(default, result.CreatedAt);

        ChatRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Chat>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new CreateChatCommand
        {
            Name = "Test Chat",
            Description = "Test Description",
            IsDefault = false,
            CreatedById = Guid.NewGuid()
        };

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(command.CreatedById, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        ChatRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Chat>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
} 