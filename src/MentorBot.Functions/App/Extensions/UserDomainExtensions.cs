using System;
using System.Collections.Generic;
using System.Linq;

using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Base;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>An extensions for the user domain object.</summary>
    public static class UserDomainExtensions
    {
        /// <summary>Finds the user by reference.</summary>
        public static User FindUserByRef(this IReadOnlyList<User> users, UserReference userRef) =>
            userRef == null ? null : users.FirstOrDefault(it => it.OpenAirUserId == userRef.OpenAirUserId);

        /// <summary>Requestors the has access to user data.</summary>
        public static bool RequestorHasAccessToUserData(this IReadOnlyList<User> users, User user, string requestorEmail) =>
            requestorEmail == null ||
            requestorEmail.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase) ||
            requestorEmail.Equals(user.Department?.Owner?.Email, StringComparison.InvariantCultureIgnoreCase) ||
            users.IsRequestorManager(user, requestorEmail);

        /// <summary>Determines whether the requestor is a manager of the specified user.</summary>
        public static bool IsRequestorManager(this IReadOnlyList<User> users, User user, string requestorEmail) =>
            users.IsUserRefManager(user?.Manager, requestorEmail, new List<string>());

        private static bool IsUserRefManager(this IReadOnlyList<User> users, UserReference userRef, string email, IList<string> emails)
        {
            if (userRef == null ||
                string.IsNullOrEmpty(userRef.Email) ||
                emails.Contains(userRef.Email))
            {
                return false;
            }

            if (userRef.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            var manager = users.FindUserByRef(userRef);
            if (manager == null)
            {
                return false;
            }

            emails.Add(userRef.Email);
            return users.IsUserRefManager(manager.Manager, email, emails);
        }
    }
}