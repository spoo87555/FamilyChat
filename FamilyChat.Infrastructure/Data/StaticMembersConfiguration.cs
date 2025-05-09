using System.Collections.Generic;

namespace FamilyChat.Infrastructure.Data;

public static class StaticMembersConfiguration
{
    public static class DefaultChat
    {
        public const string Name = "Family Chat";
        public const string Description = "Default family chat room";
    }

    public static class Members
    {
        public static readonly List<(string Email, string FirstName, string LastName, string Password)> DefaultMembers = new()
        {
            ("mom@family.com", "Mom", "Family", "Mom123!"),
            ("dad@family.com", "Dad", "Family", "Dad123!"),
            ("sister@family.com", "Sister", "Family", "Sister123!"),
            ("brother@family.com", "Brother", "Family", "Brother123!")
        };
    }
} 