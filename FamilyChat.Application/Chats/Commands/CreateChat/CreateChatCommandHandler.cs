using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyChat.Application.Chats.Commands.CreateChat;

public class CreateChatCommandHandler : ICommandHandler<CreateChatCommand, CreateChatResponse>
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateChatCommandHandler> _logger;

    public CreateChatCommandHandler(
        IChatRepository chatRepository,
        IUserRepository userRepository,
        ILogger<CreateChatCommandHandler> logger)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CreateChatResponse> Handle(CreateChatCommand command)
    {
        var creator = await _userRepository.GetByIdAsync(command.CreatedById);
        if (creator == null)
        {
            throw new InvalidOperationException($"User with ID {command.CreatedById} not found.");
        }

        var chat = new Chat(
            command.Name,
            command.Description,
            creator,
            command.IsDefault
        );

        // Add creator as a member
        chat.AddMember(creator);

        await _chatRepository.AddAsync(chat);

        _logger.LogInformation("Created chat {ChatId} by user {UserId}", chat.Id, creator.Id);

        return new CreateChatResponse
        {
            Id = chat.Id,
            Name = chat.Name,
            Description = chat.Description,
            IsDefault = chat.IsDefault,
            CreatedAt = chat.CreatedAt,
            CreatedById = chat.CreatedById
        };
    }
} 