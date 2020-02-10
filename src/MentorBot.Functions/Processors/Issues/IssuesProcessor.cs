// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Connectors.Jira;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors.Issues
{
    /// <summary>A processor that return tickets/issues information.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class IssuesProcessor : ICommandProcessor
    {
        private readonly IJiraClient _jenkinsClient;

        /// <summary>Initializes a new instance of the <see cref="IssuesProcessor"/> class.</summary>
        public IssuesProcessor(IJiraClient jenkinsClient)
        {
            _jenkinsClient = jenkinsClient;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => nameof(Issues);

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder, IPluginPropertiesAccessor accessor)
        {
            var hosts = accessor.GetPluginPropertyGroup(IssuesProperties.HostsGroup).FirstOrDefault();
            if (hosts == null)
            {
                return new ChatEventResult("No jira hosts are configured!");
            }

            var host = hosts.GetValue<string>(IssuesProperties.Host);
            var user = hosts.GetValue<string>(IssuesProperties.User);
            var token = hosts.GetValue<string>(IssuesProperties.Token);
            var project = info.Entities.GetValueOrDefault("Project")?.FirstOrDefault();
            var status = info.Entities.GetValueOrDefault("State")?.FirstOrDefault();
            if (string.IsNullOrEmpty(project) ||
                string.IsNullOrEmpty(status))
            {
                return new ChatEventResult("If you try to get Jira issues, please provide project and status!");
            }

            var result = await _jenkinsClient.QueryAsync(project, status, host, user, token);
            var widgets = result.Issues.Select(it => new WidgetMarkup
            {
                KeyValue = new KeyValue
                {
                    TopLabel = it.Key,
                    Content = it.Fields.Summary,
                    BottomLabel = it.Fields.Assignee != null ? $"Assigned to {it.Fields.Assignee.DisplayName}" : null
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
