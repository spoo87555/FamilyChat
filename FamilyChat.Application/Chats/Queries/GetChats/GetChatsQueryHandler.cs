using FamilyChat.Application.Common.Interfaces;
using FamilyChat.Domain.Interfaces;

namespace FamilyChat.Application.Chats.Queries.GetChats;

public class GetChatsQueryHandler : IQueryHandler<GetChatsQuery, IEnumerable<GetChatsResponse>>
{
    private readonly IChatRepository _chatRepository;

    public GetChatsQueryHandler(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<IEnumerable<GetChatsResponse>> Handle(GetChatsQuery query)
    {
        var chats = await _chatRepository.GetAllAsync();
        
        return chats.Select(chat => new GetChatsResponse
        {
            Id = chat.Id.ToString(),
            Name = chat.Name,
            CreatedBy = "todo",
            CreatedAt = chat.CreatedAt
        });
    }
} 