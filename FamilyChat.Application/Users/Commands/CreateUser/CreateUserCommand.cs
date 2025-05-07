using System.Threading.Tasks;
using FamilyChat.Application.Common.Interfaces;

namespace FamilyChat.Application.Users.Commands.CreateUser;

public record CreateUserCommand : ICommand<CreateUserResponse>
{
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Password { get; init; }
}

public record CreateUserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
} 