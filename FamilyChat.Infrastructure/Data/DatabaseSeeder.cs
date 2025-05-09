using System;
using System.Linq;
using System.Threading.Tasks;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyChat.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IChatRepository chatRepository,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();

            if (!await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Seeding default users...");
                foreach (var member in StaticMembersConfiguration.Members.DefaultMembers)
                {
                    var user = new User(
                        member.Email,
                        member.FirstName,
                        member.LastName,
                        member.Password // In a real app, this should be hashed
                    );
                    await _userRepository.AddAsync(user);
                }
            }

            if (!await _context.Chats.AnyAsync(c => c.IsDefault))
            {
                _logger.LogInformation("Creating default chat...");
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                if (firstUser != null)
                {
                    var defaultChat = new Chat(
                        StaticMembersConfiguration.DefaultChat.Name,
                        StaticMembersConfiguration.DefaultChat.Description,
                        firstUser,
                        true
                    );

                    // Add all users as members
                    var users = await _context.Users.ToListAsync();
                    foreach (var user in users)
                    {
                        defaultChat.AddMember(user);
                    }

                    await _chatRepository.AddAsync(defaultChat);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
} 