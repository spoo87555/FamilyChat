using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyChat.Application.Users.Queries.GetUserDetails;

public class GetUserDetailsQueryHandler : IQueryHandler<GetUserDetailsQuery, GetUserDetailsResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserDetailsQueryHandler> _logger;

    public GetUserDetailsQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserDetailsQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<GetUserDetailsResponse> Handle(GetUserDetailsQuery query)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);
        
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {query.UserId} not found.");
        }

        _logger.LogInformation("Retrieved user details for user {UserId}", user.Id);

        return new GetUserDetailsResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };
    }
} 