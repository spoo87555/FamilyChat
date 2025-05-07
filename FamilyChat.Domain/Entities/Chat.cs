using System;
using System.Collections.Generic;

namespace FamilyChat.Domain.Entities;

public class Chat
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedById { get; private set; }
    public User CreatedBy { get; private set; }
    public ICollection<ChatMember> Members { get; private set; }
    public ICollection<Message> Messages { get; private set; }

    private Chat() { } // For EF Core

    public Chat(string name, string description, User createdBy, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Chat name cannot be empty", nameof(name));
        if (createdBy == null)
            throw new ArgumentNullException(nameof(createdBy));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
        Members = new List<ChatMember>();
        Messages = new List<Message>();
    }

    public void AddMember(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (Members.Any(m => m.UserId == user.Id))
            throw new InvalidOperationException("User is already a member of this chat");

        Members.Add(new ChatMember(this, user));
    }

    public void RemoveMember(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var member = Members.FirstOrDefault(m => m.UserId == user.Id);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this chat");

        Members.Remove(member);
    }
} 