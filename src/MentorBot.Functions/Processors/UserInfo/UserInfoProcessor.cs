using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

using CardBody = Google.Apis.HangoutsChat.v1.Data.KeyValue;

namespace MentorBot.Functions.Processors.UserInfo
{
    /// <summary>A user information processor.</summary>
    public sealed class UserInfoProcessor : ICommandProcessor
    {
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="UserInfoProcessor"/> class.</summary>
        public UserInfoProcessor(IStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
#pragma warning disable CC0021
        public string Subject => @"User";
#pragma warning restore CC0021

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var userName = info.Entities.GetValueOrDefault("Text")?.FirstOrDefault();
            var senderEmail = originalChatEvent?.Message?.Sender.Email;
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new ChatEventResult("User was not found!");
            }

            var users = await _storageService.GetAllUsersAsync();
            var foundUsers = users
                .Where(u => MatchUserByName(u, userName))
                .Where(u => users.RequestorHasAccessToUserData(u, senderEmail))
                .ToArray();

            if (foundUsers.Length == 0)
            {
                return new ChatEventResult($"User with name {userName} was not found!");
            }

            return new ChatEventResult(
                foundUsers.Select(user =>
                        ChatEventFactory.CreateCard(
                            new CardBody
                            {
                                Icon = "PERSON",
                                TopLabel = user.Name,
                                Content = $"{user.Email}\n_{user.Department?.Name}, {FindManagerName(users, user.Manager?.OpenAirUserId)}_",
                                ContentMultiline = true,
                                BottomLabel = string.Join(", ", user.Customers?.Select(it => it.Name) ?? new string[0]),
                            }))
                    .ToArray());
        }

        private static string FindManagerName(IReadOnlyList<User> users, long? openAirId) =>
            openAirId.HasValue ? users.FirstOrDefault(user => user.OpenAirUserId == openAirId.Value)?.Name : null;

        private static bool MatchUserByName(User user, string name)
        {
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                return false;
            }

            return
                user.Email.StartsWith(name, StringComparison.InvariantCultureIgnoreCase) ||
                MatchNamesByName(
                    user.Name.Split(',').Where(it => it != null).Select(it => it.Trim()).ToArray(),
                    name);
        }

        private static bool MatchNamesByName(string[] names, string name)
        {
            switch (names.Length)
            {
                case 1:
                    return names[0].Equals(name, StringComparison.InvariantCultureIgnoreCase);
                case 2:
                    var firstOrLastName = $"({names[0]}|{names[1]})";
                    var startWithFirstNameMatch =
                        new Regex($"{firstOrLastName}( , | . | ){firstOrLastName}( @ mentormate \\. (com|net)|)$", RegexOptions.IgnoreCase);
                    return startWithFirstNameMatch.IsMatch(name);

                default:
                    return false;
            }
        }
    }
}
