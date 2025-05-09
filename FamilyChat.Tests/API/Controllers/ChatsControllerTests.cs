using System;
using System.Net;
using System.Threading.Tasks;
using FamilyChat.Application.Chats.Commands.CreateChat;
using FamilyChat.Tests.API.Common;
using Xunit;

namespace FamilyChat.Tests.API.Controllers;

public class ChatsControllerTests : ApiTestBase
{
    public ChatsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateChat_ValidData_ShouldReturnCreatedChat()
    {
        // Arrange
        var command = new CreateChatCommand
        {
            Name = "Test Chat",
            Description = "Test Description",
            IsDefault = false,
            CreatedById = Guid.NewGuid()
        };

        var content = CreateJsonContent(command);

        // Act
        var response = await Client.PostAsync("/api/Chats", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await DeserializeResponse<CreateChatResponse>(response);

        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.IsDefault, result.IsDefault);
        Assert.Equal(command.CreatedById, result.CreatedById);
        Assert.NotEqual(default, result.Id);
        Assert.NotEqual(default, result.CreatedAt);

        // Verify database state
        var chat = await DbContext.Chats.FindAsync(result.Id);
        Assert.NotNull(chat);
        Assert.Equal(command.Name, chat.Name);
        Assert.Equal(command.Description, chat.Description);
        Assert.Equal(command.IsDefault, chat.IsDefault);
        Assert.Equal(command.CreatedById, chat.CreatedById);
    }

    [Fact]
    public async Task CreateChat_InvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateChatCommand
        {
            Name = "", // Invalid empty name
            Description = "Test Description",
            IsDefault = false,
            CreatedById = Guid.NewGuid()
        };

        var content = CreateJsonContent(command);

        // Act
        var response = await Client.PostAsync("/api/Chats", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 