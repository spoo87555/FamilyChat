using FamilyChat.Domain.Entities;
using FamilyChat.Infrastructure.Data;
using FamilyChat.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace FamilyChat.Tests.Integration.Repositories;

public class UserRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidUser_ShouldAddToDatabase()
    {
        // Arrange
        var user = new User
        (
            "test@example.com",
            "John",
            "Doe",
            "hashed_password"
        );

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(user.Email);
        savedUser.FirstName.Should().Be(user.FirstName);
        savedUser.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        (
            "test@example.com",
            "John",
            "Doe",
            "hashed_password"
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var foundUser = await _repository.GetByEmailAsync(user.Email);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingUser_ShouldReturnNull()
    {
        // Act
        var user = await _repository.GetByEmailAsync("nonexisting@example.com");

        // Assert
        user.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
} 