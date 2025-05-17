using System;
using System.Threading.Tasks;
using FamilyChat.Application.Chats.Commands.CreateChat;
using FamilyChat.Application.Chats.Queries.GetChats;
using FamilyChat.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FamilyChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatsController : ApiControllerBase
{
    private readonly ICommandHandler<CreateChatCommand, CreateChatResponse> _createChatHandler;
    private readonly IQueryHandler<GetChatsQuery, IEnumerable<GetChatsResponse>> _getChatsHandler;
    private readonly ILogger<ChatsController> _logger;

    public ChatsController(
        ICommandHandler<CreateChatCommand, CreateChatResponse> createChatHandler,
        IQueryHandler<GetChatsQuery, IEnumerable<GetChatsResponse>> getChatsHandler,
        ILogger<ChatsController> logger)
    {
        _createChatHandler = createChatHandler;
        _getChatsHandler = getChatsHandler;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available chats
    /// </summary>
    /// <returns>List of chats</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(IEnumerable<GetChatsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GetChatsResponse>>> GetAll()
    {
        try
        {
            var query = new GetChatsQuery();
            var result = await _getChatsHandler.Handle(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chats");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving chats");
        }
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