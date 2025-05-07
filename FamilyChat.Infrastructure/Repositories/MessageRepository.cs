using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Infrastructure.Data;

namespace FamilyChat.Infrastructure.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Message>> GetByChatIdAsync(Guid chatId, int skip = 0, int take = 50)
    {
        return await _dbSet
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(m => m.Chat)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByChatIdAsync(Guid chatId)
    {
        return await _dbSet.CountAsync(m => m.ChatId == chatId);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(m => m.Id == id);
    }

    public override async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Chat)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
} 