using System;

namespace FamilyChat.Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public bool IsEdited { get; private set; }
    public Guid ChatId { get; private set; }
    public Chat Chat { get; private set; }
    public Guid SenderId { get; private set; }
    public User Sender { get; private set; }

    private Message() { } // For EF Core

    public Message(Guid chatId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty", nameof(content));
        if (chatId == null)
            throw new ArgumentNullException(nameof(chatId));
        if (senderId == null)
            throw new ArgumentNullException(nameof(senderId));

        Id = Guid.NewGuid();
        Content = content;
        CreatedAt = DateTime.UtcNow;
        IsEdited = false;
        ChatId = chatId;
        SenderId = senderId;
    }

    public void Edit(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Message content cannot be empty", nameof(newContent));

        Content = newContent;
        EditedAt = DateTime.UtcNow;
        IsEdited = true;
    }
} 