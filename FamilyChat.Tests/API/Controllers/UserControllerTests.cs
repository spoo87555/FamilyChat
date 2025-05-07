using System.Net.Http.Json;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyChat.Tests.API.Controllers;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        // Get the DbContext from the factory's service provider
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [Fact]
    public async Task CreateUser_ValidData_ShouldReturnCreatedUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);

        // Verify the user was actually saved to the database
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task CreateUser_InvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "invalid-email",
            FirstName = "",
            LastName = "",
            Password = "123" // Too short
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123!"
        };

        // Create first user
        await _client.PostAsJsonAsync("/api/users", command);

        // Act - Try to create second user with same email
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
} 