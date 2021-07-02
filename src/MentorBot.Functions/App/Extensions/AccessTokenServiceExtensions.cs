using System;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;

using Microsoft.Azure.Functions.Worker.Http;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>Extends the IAccessTokenService functionality.</summary>
    public static class AccessTokenServiceExtensions
    {
        /// <summary>Ensure the user is in a specific role.</summary>
        public static Task EnsureRole(this IAccessTokenService accessTokenService, HttpRequestData req, UserRoles role) =>
            accessTokenService
                .ValidateTokenAsync(req)
                .ContinueWith(task =>
                {
                    if (!task.Result.IsValid ||
                        (role & task.Result.UserRole) == 0)
                    {
                        throw new AccessViolationException("The provided user do not have access to the resource!");
                    }
                });
    }
}
