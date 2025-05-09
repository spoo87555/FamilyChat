using System;
using System.Threading.Tasks;
using FamilyChat.Application.Chats.Commands.CreateChat;
using FamilyChat.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FamilyChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ApiControllerBase
{
    private readonly ICommandHandler<CreateChatCommand, CreateChatResponse> _createChatHandler;
    private readonly ILogger<ChatsController> _logger;

    public ChatsController(
        ICommandHandler<CreateChatCommand, CreateChatResponse> createChatHandler,
        ILogger<ChatsController> logger)
    {
        _createChatHandler = createChatHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateChatResponse>> Create(CreateChatCommand command)
    {
        try
        {
            var result = await _createChatHandler.Handle(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create chat");
            return BadRequest(ex.Message);
        }
    }
} 