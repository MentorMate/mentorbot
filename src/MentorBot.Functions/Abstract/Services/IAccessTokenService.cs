// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Functions.Models.DataResultModels;

using Microsoft.AspNetCore.Http;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>Get user information based on access token.</summary>
    public interface IAccessTokenService
    {
        /// <summary>Validate the access token in the request and return user info.</summary>
        Task<AccessTokenUserInfo> ValidateTokenAsync(HttpRequest request);
    }
}
