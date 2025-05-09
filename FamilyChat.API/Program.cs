using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Application.Users.Queries.GetUserDetails;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Infrastructure.Data;
using FamilyChat.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyChat.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        // Add DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IChatRepository, ChatRepository>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();

        // Register command and query handlers
        builder.Services.AddScoped<ICommandHandler<CreateUserCommand, CreateUserResponse>, CreateUserCommandHandler>();
        builder.Services
            .AddScoped<IQueryHandler<GetUserDetailsQuery, GetUserDetailsResponse>, GetUserDetailsQueryHandler>();

        // Register database seeder
        builder.Services.AddScoped<DatabaseSeeder>();

        var app = builder.Build();

        // Seed the database
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}