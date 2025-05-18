using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FamilyChat.Domain.Interfaces;
using FamilyChat.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace FamilyChat.API.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatRepository _chatRepository;

    public ChatHub(
        ILogger<ChatHub> logger,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IChatRepository chatRepository)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
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