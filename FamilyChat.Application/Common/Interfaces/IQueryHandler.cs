using System.Threading.Tasks;

namespace FamilyChat.Application.Common.Interfaces;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query);
} 