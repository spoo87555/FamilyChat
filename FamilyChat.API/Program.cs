using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Application.Users.Queries.GetUserDetails;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Infrastructure.Data;
using FamilyChat.Infrastructure.Repositories;
using FamilyChat.API.Hubs;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.OpenApi.Models;

namespace FamilyChat.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;


        // Add authentication
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));
        
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
                           .AllowCredentials()
                           .SetIsOriginAllowed(origin => true); // For development only
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

            // Add OAuth2 authentication to Swagger
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{builder.Configuration["AzureAdB2C:Instance"]}/{builder.Configuration["AzureAdB2C:Domain"]}/{builder.Configuration["AzureAdB2C:SignUpSignInPolicyId"]}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"{builder.Configuration["AzureAdB2C:Instance"]}/{builder.Configuration["AzureAdB2C:Domain"]}/{builder.Configuration["AzureAdB2C:SignUpSignInPolicyId"]}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" }
                        }
                    }
                }
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    new[] { "openid", "profile", "email" }
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
                c.OAuthClientId(builder.Configuration["AzureAdB2C:ClientId"]);
                c.OAuthScopes("openid", "profile", "email");
                c.OAuth2RedirectUrl("https://localhost:7296/swagger/oauth2-redirect.html");
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