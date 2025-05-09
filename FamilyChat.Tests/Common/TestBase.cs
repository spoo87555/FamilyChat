using System;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyChat.Tests.Common;

public abstract class TestBase
{
    protected readonly Mock<IChatRepository> ChatRepositoryMock;
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Mock<ILogger> LoggerMock;

    protected TestBase()
    {
        ChatRepositoryMock = new Mock<IChatRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        LoggerMock = new Mock<ILogger>();
    }

    protected User CreateTestUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };
    }

    protected Chat CreateTestChat(Guid? id = null, User creator = null)
    {
        creator ??= CreateTestUser();
        return new Chat(
            "Test Chat",
            "Test Description",
            creator,
            false
        );
    }
} 