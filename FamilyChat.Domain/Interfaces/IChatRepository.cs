using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyChat.Domain.Entities;

namespace FamilyChat.Domain.Interfaces;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id);
    Task<IEnumerable<Chat>> GetByUserIdAsync(Guid userId);
    Task<Chat?> GetDefaultChatAsync();
    Task<IEnumerable<Chat>> GetAllAsync();
    Task AddAsync(Chat chat);
    Task UpdateAsync(Chat chat);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> IsUserMemberAsync(Guid chatId, Guid userId);
} 