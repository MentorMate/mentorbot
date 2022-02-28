using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Connectors.Confluence;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors.Searches
{
    /// <summary>A processor that return search information.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class SearchesProcessor : ICommandProcessor
    {
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000", Justification = "new format")]
        private static readonly Regex Exp = new(
            "^(What +|Where +|Who +|Where +|are +|is +)+([\\w\\d\\s\\,\\.]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly IConfluenceClient _client;

        /// <summary>Initializes a new instance of the <see cref="SearchesProcessor"/> class.</summary>
        public SearchesProcessor(IConfluenceClient confluenceClient)
        {
            _client = confluenceClient;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => "Encyclopedia";

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var query = GetQueryText(info);
            if (string.IsNullOrEmpty(query))
            {
                return new ChatEventResult("I do not know the answer to that!");
            }

            try
            {
                var hosts = accessor.GetPluginPropertyGroup(SearchesProperties.HostsGroup).FirstOrDefault();
                if (hosts == null)
                {
                    return new ChatEventResult("No atlassian hosts are configured!");
                }

                var user = hosts.GetValue<string>(SearchesProperties.User);
                var token = hosts.GetValue<string>(SearchesProperties.Token);

                var result = await _client.QueryAsync(query, user, token);

                var cards = new Card[result.Results.Length];
                var index = 0;

                foreach (var searchResult in result.Results)
                {
                    var card = new Card
                    {
                        Header = new CardHeader
                        {
                            Title = searchResult.Title,
                        },
                        Sections = new List<Section>
                    {
                        new Section
                        {
                            Widgets = new[]
                            {
                                new WidgetMarkup
                                {
                                    TextParagraph = new TextParagraph
                                    {
                                        Text = result.Links.Base + searchResult.Links.WebUi,
                                    }
                                }
                            }
                        }
                    },
                    };

                    var pagePath = result.Links.Self;
                    if (!string.IsNullOrEmpty(pagePath))
                    {
                        card.Sections.Add(
                            new Section
                            {
                                Widgets = new[]
                                {
                                new WidgetMarkup
                                {
                                    Buttons = new[]
                                    {
                                        ChatEventFactory.CreateTextButton("Confluence", pagePath)
                                    }
                                }
                                }
                            });
                    }

                    cards[index++] = card;
                }

                return new ChatEventResult(cards);
            }
            catch (HttpRequestException ex)
            {
                Debug.Write(ex.Message);
                return new ChatEventResult("I do not know the answer to that!");
            }
            catch (Exception ex)
            {
                return new ChatEventResult("Unknown error occurred:" + ex.Message);
            }
        }

        private static string GetQueryText(TextDeconstructionInformation info)
        {
            var entity =
                info.Entities.GetValueOrDefault("Query", null) ??
                info.Entities.GetValueOrDefault("Text", null) ??
                info.Entities.GetValueOrDefault("Person Name", null);

            return entity == null || entity.Length == 0 ?
                Exp.Match(info.TextSentenceChunk)?.Groups[2]?.Value :
                string.Join(' ', entity);
        }
    }
}
