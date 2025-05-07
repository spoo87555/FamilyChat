using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyChat.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand command)
    {
        if (await _userRepository.ExistsByEmailAsync(command.Email))
        {
            throw new InvalidOperationException($"User with email {command.Email} already exists.");
        }

        // In a real application, we would hash the password here
        var user = new User(
            command.Email,
            command.FirstName,
            command.LastName,
            command.Password // This should be hashed in production
        );

        await _userRepository.AddAsync(user);

        _logger.LogInformation("Created user {UserId} with email {Email}", user.Id, user.Email);

        return new CreateUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
} 