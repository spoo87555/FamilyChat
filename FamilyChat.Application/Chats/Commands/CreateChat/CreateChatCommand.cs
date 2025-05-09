using FamilyChat.Application.Common.Interfaces;

namespace FamilyChat.Application.Chats.Commands.CreateChat;

public record CreateChatCommand : ICommand<CreateChatResponse>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public bool IsDefault { get; init; }
    public Guid CreatedById { get; init; }
}

public record CreateChatResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedById { get; init; }
} 