using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Application.Users.Commands.CreateUser;
using FamilyChat.Application.Users.Queries.GetUserDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FamilyChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
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

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="command">User creation details</param>
    /// <returns>The created user's details</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the command is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateUserResponse>> CreateUser(CreateUserCommand command)
    {
        try
        {
            var result = await _createUserHandler.Handle(command);
            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create user");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets user details by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user's details</returns>
    /// <response code="200">Returns the user details</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetUserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetUserDetailsResponse>> GetUser(Guid id)
    {
        var user = HttpContext?.User!;
        try
        {
            var query = new GetUserDetailsQuery { UserId = id };
            var result = await _getUserDetailsHandler.Handle(query);
            
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to get user details for user {UserId}", id);
            return NotFound(ex.Message);
        }
    }
} 