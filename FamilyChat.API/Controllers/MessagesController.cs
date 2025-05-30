using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FamilyChat.API.Hubs;
using FamilyChat.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FamilyChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IMessageRepository messageRepository,
        IChatRepository chatRepository,
        IUserRepository userRepository,
        IHubContext<ChatHub> hubContext,
        ILogger<MessagesController> logger)
    {
        _messageRepository = messageRepository;
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets messages for a specific chat
    /// </summary>
    /// <param name="chatId">The chat ID</param>
    /// <param name="skip">Number of messages to skip</param>
    /// <param name="take">Number of messages to take</param>
    /// <returns>List of messages</returns>
    [HttpGet("chat/{chatId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatMessages(Guid chatId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        if (!await _chatRepository.ExistsAsync(chatId))
            return NotFound($"Chat with ID {chatId} not found");

        var messages = await _messageRepository.GetByChatIdAsync(chatId, skip, take);
        return Ok(messages);
    }

    /// <summary>
    /// Sends a message to a chat
    /// </summary>
    /// <param name="chatId">The chat ID</param>
    /// <param name="content">Message content</param>
    /// <returns>The created message</returns>
    [HttpPost("chat/{chatId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("Message content cannot be empty");

        if (!await _chatRepository.ExistsAsync(chatId))
            return NotFound($"Chat with ID {chatId} not found");

        // Get the user's email from claims
        var userEmail = User.FindFirst("emails")?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("Email claim not found in token. Available claims: {Claims}", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
            return Unauthorized("User email not found in claims");
        }

        // Get or create user based on email
        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user == null) 
        {
            // Create new user from claims
            user = new User(
                userEmail,
                User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Unknown",
                User.FindFirst(ClaimTypes.Surname)?.Value ?? "User",
                null // No password hash needed for social login
            );
            await _userRepository.AddAsync(user);
        }

        var message = new Message(chatId, user.Id, content);
        await _messageRepository.AddAsync(message);

        // Notify all clients in the chat group
        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", new
        {
            id = message.Id,
            chatId = message.ChatId,
            content = message.Content,
            senderName = $"{user.FirstName} {user.LastName}",
            senderId = message.SenderId,
            createdAt = message.CreatedAt,
            editedAt = message.EditedAt,
            isEdited = message.IsEdited,
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
        });

        return CreatedAtAction(nameof(GetChatMessages), new { chatId }, message);
    }

    /// <summary>
    /// Gets a specific message by ID
    /// </summary>
    /// <param name="id">The message ID</param>
    /// <returns>The message details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessage(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);
        if (message == null)
            return NotFound($"Message with ID {id} not found");

        return Ok(message);
    }
} 