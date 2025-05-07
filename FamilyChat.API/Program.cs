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
    public static void Main(string[] args)
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

        var app = builder.Build();

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