using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Application.Users.Queries.GetUserDetails;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Infrastructure.Data;
using FamilyChat.Infrastructure.Repositories;
using FamilyChat.API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace FamilyChat.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

        // Add JWT Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        // Add services to the container.
        builder.Services.AddControllers();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                builder =>
                {
                    builder.WithOrigins("http://localhost:5173") // React dev server
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
        });

        // Configure Swagger/OpenAPI with authentication
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

            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Add SignalR
        builder.Services.AddSignalR();

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
        
        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => false;
            options.Secure = CookieSecurePolicy.Always;
            options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
        });
        
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

        app.UseCookiePolicy();
        
        // Use CORS before authentication
        app.UseCors("AllowReactApp");
        
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure SignalR hub
        app.MapHub<ChatHub>("/hubs/chat");

        app.MapControllers();

        app.Run();
    }
}