using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Application.Users.Queries.GetUserDetails;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Infrastructure.Data;
using FamilyChat.Infrastructure.Repositories;
using FamilyChat.API.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace FamilyChat.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Configure Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Family Chat API",
                Version = "v1",
                Description = "API for the Family Chat application",
                Contact = new OpenApiContact
                {
                    Name = "Family Chat Team",
                    Email = "support@familychat.com"
                }
            });
        });

        // Add SignalR
        builder.Services.AddSignalR();

        builder.Services.AddAuthorization();

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
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Family Chat API V1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthorization();

        // Configure SignalR hub
        app.MapHub<ChatHub>("/hubs/chat");

        app.MapControllers();

        app.Run();
    }
}