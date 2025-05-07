using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Infrastructure.Data;

namespace FamilyChat.Infrastructure.Repositories;

public class ChatRepository : BaseRepository<Chat>, IChatRepository
{
    public ChatRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Chat>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(c => c.Members)
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<Chat?> GetDefaultChatAsync()
    {
        return await _dbSet
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.IsDefault);
    }

    public async Task<bool> IsUserMemberAsync(Guid chatId, Guid userId)
    {
        return await _dbSet
            .AnyAsync(c => c.Id == chatId && c.Members.Any(m => m.UserId == userId));
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(c => c.Id == id);
    }

    public override async Task<Chat?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Members)
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
} 