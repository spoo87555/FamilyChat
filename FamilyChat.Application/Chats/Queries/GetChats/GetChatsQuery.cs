using FamilyChat.Application.Common.Interfaces;

namespace FamilyChat.Application.Chats.Queries.GetChats;

public class GetChatsQuery : IQuery<IEnumerable<GetChatsResponse>>
{
    // No parameters needed for now, we'll get all chats
} 