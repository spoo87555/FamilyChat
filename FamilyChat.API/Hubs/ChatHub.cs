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

    public async Task SendMessage(Guid chatId, string content)
    {
        try
        {
            _logger.LogInformation("SendMessage called with chatId: {ChatId}, content: {Content}", chatId, content);

            // Get user email from claims
            var userEmail = Context.User?.FindFirst("emails")?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Email claim not found in token");
                return;
            }
            _logger.LogInformation("User email from claims: {Email}", userEmail);

            // Get or create user
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", userEmail);
                return;
            }
            _logger.LogInformation("User found: {UserId}, {FirstName} {LastName}", user.Id, user.FirstName, user.LastName);

            // Verify chat exists
            var chat = await _chatRepository.GetByIdAsync(chatId);
            if (chat == null)
            {
                _logger.LogWarning("Chat not found: {ChatId}", chatId);
                return;
            }
            _logger.LogInformation("Chat found: {ChatId}", chatId);

            // Create and save message
            var message = new Message(chatId, user.Id, content);
            await _messageRepository.AddAsync(message);
            _logger.LogInformation("Message created and saved: {MessageId}", message.Id);

            // Get the complete message with sender details
            var completeMessage = await _messageRepository.GetByIdAsync(message.Id);
            if (completeMessage == null)
            {
                _logger.LogError("Failed to retrieve created message");
                return;
            }
            _logger.LogInformation("Complete message retrieved: {MessageId}", completeMessage.Id);

            var messageToSend = new
            {
                id = completeMessage.Id,
                chatId = completeMessage.ChatId,
                content = completeMessage.Content,
                senderName = $"{user.FirstName} {user.LastName}",
                senderId = completeMessage.SenderId,
                createdAt = completeMessage.CreatedAt,
                editedAt = completeMessage.EditedAt,
                isEdited = completeMessage.IsEdited,
                sender = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    createdAt = user.CreatedAt,
                    lastLoginAt = user.LastLoginAt,
                    isActive = user.IsActive,
                    deviceToken = user.DeviceToken
                }
            };

            _logger.LogInformation("Sending message to group: {Message}", JsonSerializer.Serialize(messageToSend));

            // Send message to all clients in the chat group
            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageToSend);

            _logger.LogInformation("Message sent successfully to chat {ChatId} by user {UserId}", chatId, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
            throw;
        }
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