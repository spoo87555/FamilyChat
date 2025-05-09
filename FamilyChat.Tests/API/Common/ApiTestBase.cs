using System;
using System.Net.Http;
using System.Text.Json;
using FamilyChat.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyChat.Tests.API.Common;

public abstract class ApiTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;
    protected readonly JsonSerializerOptions JsonOptions;

    protected ApiTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected StringContent CreateJsonContent<T>(T data)
    {
        return new StringContent(
            JsonSerializer.Serialize(data, JsonOptions),
            System.Text.Encoding.UTF8,
            "application/json");
    }

    protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
} 