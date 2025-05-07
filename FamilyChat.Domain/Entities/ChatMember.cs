using System;

namespace FamilyChat.Domain.Entities;

public class ChatMember
{
    public Guid ChatId { get; private set; }
    public Chat Chat { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsAdmin { get; private set; }

    private ChatMember() { } // For EF Core

    public ChatMember(Chat chat, User user, bool isAdmin = false)
    {
        if (chat == null)
            throw new ArgumentNullException(nameof(chat));
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        ChatId = chat.Id;
        Chat = chat;
        UserId = user.Id;
        User = user;
        JoinedAt = DateTime.UtcNow;
        IsAdmin = isAdmin;
    }

    public void MakeAdmin()
    {
        IsAdmin = true;
    }

    public void RemoveAdmin()
    {
        IsAdmin = false;
    }
} 