using System.Threading.Tasks;

namespace FamilyChat.Application.Common.Interfaces;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command);
} 