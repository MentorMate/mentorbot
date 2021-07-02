// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors.BuildInfo
{
    /// <summary>A processor that return source build information.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class BuildInfoProcessor : ICommandProcessor
    {
        private readonly IJenkinsClient _jenkinsClient;

        /// <summary>Initializes a new instance of the <see cref="BuildInfoProcessor"/> class.</summary>
        public BuildInfoProcessor(IJenkinsClient jenkinsClient)
        {
            _jenkinsClient = jenkinsClient;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => nameof(BuildInfo);

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var jobNames = await accessor.GetAllUserPropertyValuesAsync<string>(BuildInfoProperties.JobName);
            if (jobNames == null ||
                jobNames.Count == 0)
            {
                return new ChatEventResult("No jobs are assign to you!");
            }

            var hosts = accessor.GetPluginPropertyGroup(BuildInfoProperties.HostsGroup).FirstOrDefault();
            if (hosts == null)
            {
                return new ChatEventResult("No jenkins hosts are configured!");
            }

            var host = hosts.GetValue<string>(BuildInfoProperties.Host);
            var user = hosts.GetValue<string>(BuildInfoProperties.User);
            var token = hosts.GetValue<string>(BuildInfoProperties.Token);
            var jenkinsResults = await Task.WhenAll(
                jobNames.Select(jobName =>
                    _jenkinsClient.QueryAsync(jobName, host, user, token)));

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
                        },
                    },
                });
        }
    }
}
