using System;
using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;

namespace FamilyChat.Application.Users.Queries.GetUserDetails;

public record GetUserDetailsQuery : IQuery<GetUserDetailsResponse>
{
    public Guid UserId { get; init; }
}

public record GetUserDetailsResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsActive { get; init; }
} 