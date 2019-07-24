using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

namespace MentorBot.Functions.Services
{
    /// <summary>The access token service for google.</summary>
    public sealed class GoogleAccessTokenService : IAccessTokenService
    {
        private const string DefaultGoogleAuthTokenInfoApi = "https://www.googleapis.com/oauth2/v1/tokeninfo";
        private const string DefaultAuthenticationSchema = "Bearer";

        #pragma warning disable CA2213
        private readonly IMemoryCache _cache;
        #pragma warning restore CA2213

        private readonly IStorageService _storageService;
        private readonly Func<HttpMessageHandler> _messageHandlerFactory;

        /// <summary>Initializes a new instance of the <see cref="GoogleAccessTokenService"/> class.</summary>
        public GoogleAccessTokenService(IMemoryCache cache, IStorageService storageService)
            : this(cache, storageService, () => new HttpClientHandler())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GoogleAccessTokenService"/> class.</summary>
        public GoogleAccessTokenService(IMemoryCache cache, IStorageService storageService, Func<HttpMessageHandler> messageHandlerFactory)
        {
            _cache = cache;
            _storageService = storageService;
            _messageHandlerFactory = messageHandlerFactory;
        }

        /// <inheritdoc/>
        public async Task<AccessTokenUserInfo> ValidateTokenAsync(HttpRequest request)
        {
            var accessTokenInfo = await GetValidAccessTokenInfoAsync(request);
            if (accessTokenInfo == null)
            {
                return new AccessTokenUserInfo { IsValid = false };
            }

            var userRole = await _cache.GetOrCreateAsync(
                "UserRole_" + accessTokenInfo.Email,
                async entry =>
                {
                    var user = await _storageService.GetUserByEmailAsync(accessTokenInfo.Email);
                    if (user == null)
                    {
                        return UserRoles.None;
                    }

                    if (string.IsNullOrEmpty(user.GoogleUserId))
                    {
                        user.GoogleUserId = accessTokenInfo.UserId;
                        await _storageService.UpdateUsersAsync(new[] { user });
                    }

                    return (UserRoles)user.Role;
                });

            return new AccessTokenUserInfo
            {
                UserRole = userRole,
                IsValid = true
            };
        }

        private async Task<GoogleAccessTokenInfo> GetValidAccessTokenInfoAsync(HttpRequest request)
        {
            var authorizationHeader = AuthenticationHeaderValue.Parse(request.Headers[HeaderNames.Authorization]);
            if (!authorizationHeader.Scheme.Equals(DefaultAuthenticationSchema, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var accessTokenInfo = await _cache.GetOrCreateAsync(
                "UserAccessToken_" + authorizationHeader.Parameter,
                async entry =>
                {
                    var info = await ExecuteRequestAsync(authorizationHeader.Parameter, _messageHandlerFactory);
                    var expireIn = TimeSpan.FromSeconds(info.Expires);

                    entry.AbsoluteExpirationRelativeToNow = expireIn;
                    info.DueDate = DateTime.UtcNow + expireIn;

                    return info;
                });

            return
                accessTokenInfo != null &&
                accessTokenInfo.DueDate >= DateTime.UtcNow &&
                accessTokenInfo.Type == "online" &&
                !string.IsNullOrEmpty(accessTokenInfo.Email) ?
                accessTokenInfo :
                null;
        }

        private async Task<GoogleAccessTokenInfo> ExecuteRequestAsync(string accessToken, Func<HttpMessageHandler> httpMessageHandlerFactory)
        {
            using (var messageHandler = httpMessageHandlerFactory())
            using (var client = new HttpClient(messageHandler, false))
            {
                var response = await client.GetAsync(DefaultGoogleAuthTokenInfoApi + "?access_token=" + accessToken);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsAsync<GoogleAccessTokenInfo>();
            }
        }
    }
}
