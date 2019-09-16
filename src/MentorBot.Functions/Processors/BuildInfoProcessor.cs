using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A processor that return source build information.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class BuildInfoProcessor : ICommandProcessor
    {
        /// <summary>The comman processor name.</summary>
        public const string CommandName = "Build Info Processor";

        private readonly IJenkinsClient _jenkinsClient;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="BuildInfoProcessor"/> class.</summary>
        public BuildInfoProcessor(
            IJenkinsClient jenkinsClient,
            IStorageService storageService)
        {
            _jenkinsClient = jenkinsClient;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public string Name => CommandName;

        /// <inheritdoc/>
        public string Subject => "BuildInfo";

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder, IReadOnlyDictionary<string, string> settings)
        {
            var user = await _storageService.GetUserByEmailAsync(originalChatEvent.Message.Sender.Email);
            var jobNames = user?.Properties?.GetValueOrDefault("JenkinsJobs")?.Cast<string>();
            if (jobNames == null)
            {
                return new ChatEventResult("No jobs are assign to you!");
            }

            var jenkinsResults = await Task.WhenAll(
                jobNames.Select(jobName =>
                    _jenkinsClient.QueryAsync(jobName, settings[Default.HostKey], settings[Default.UsernameKey], settings[Default.TokenKey])));

            var widgets = jenkinsResults.Select(it => new WidgetMarkup
            {
                KeyValue = new KeyValue
                {
                    TopLabel = it.DisplayName,
                    Content = string.Join(", ", it.ChangeSet?.Items?.Select(cs => cs.Comment)),
                    BottomLabel = it.Result,
                    Button = ChatEventFactory.CreateTextButton("Link", it.Url),
                },
            }).ToList();

            return new ChatEventResult(
                new Card
                {
                    Sections = new[]
                    {
                        new Section
                        {
                             Widgets = widgets
                        }
                    }
                });
        }
    }
}
