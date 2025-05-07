using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Application.Users.Queries.GetUserDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FamilyChat.API.Controllers;

public class UsersController : ApiControllerBase
{
    private readonly ICommandHandler<CreateUserCommand, CreateUserResponse> _createUserHandler;
    private readonly IQueryHandler<GetUserDetailsQuery, GetUserDetailsResponse> _getUserDetailsHandler;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        ICommandHandler<CreateUserCommand, CreateUserResponse> createUserHandler,
        IQueryHandler<GetUserDetailsQuery, GetUserDetailsResponse> getUserDetailsHandler,
        ILogger<UsersController> logger)
    {
        _createUserHandler = createUserHandler;
        _getUserDetailsHandler = getUserDetailsHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserResponse>> Create(CreateUserCommand command)
    {
        try
        {
            var result = await _createUserHandler.Handle(command);
            return HandleResult(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create user");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserDetailsResponse>> GetDetails(Guid id)
    {
        try
        {
            var query = new GetUserDetailsQuery { UserId = id };
            var result = await _getUserDetailsHandler.Handle(query);
            return HandleResult(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to get user details for user {UserId}", id);
            return NotFound(ex.Message);
        }
    }
} 