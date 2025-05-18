using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyChat.Domain.Entities;

namespace FamilyChat.Domain.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id);
    Task<IEnumerable<Message>> GetByChatIdAsync(Guid chatId, int skip = 0, int? take = null);
    Task<IEnumerable<Message>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Message message);
    Task UpdateAsync(Message message);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCountByChatIdAsync(Guid chatId);
} 