using System.Threading.Tasks;

using MentorBot.Functions.Models.DataResultModels;

using Microsoft.Azure.Functions.Worker.Http;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>Get user information based on access token.</summary>
    public interface IAccessTokenService
    {
        /// <summary>Validate the access token in the request and return user info.</summary>
        Task<AccessTokenUserInfo> ValidateTokenAsync(HttpRequestData request);
    }
}
