using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FamilyChat.API.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        _logger.LogInformation("User {ConnectionId} joined chat {ChatId}", Context.ConnectionId, chatId);
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        _logger.LogInformation("User {ConnectionId} left chat {ChatId}", Context.ConnectionId, chatId);
    }

    public async Task SendMessage(Guid chatId, string content)
    {
        _logger.LogInformation("User {ConnectionId} sent message to chat {ChatId}", Context.ConnectionId, chatId);
        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", new
        {
            ChatId = chatId,
            Content = content,
            SenderId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
} 