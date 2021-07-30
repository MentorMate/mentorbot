using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.Wikipedia;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A command that search for information in the wikipedia encyclopedia.</summary>
    public sealed class WikipediaProcessor : ICommandProcessor
    {
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000", Justification = "new format")]
        private static readonly Regex Exp = new(
            "^(What +|Where +|Who +|Where +|are +|is +)+([\\w\\d\\s\\,\\.]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly IWikiClient _client;

        /// <summary>Initializes a new instance of the <see cref="WikipediaProcessor"/> class.</summary>
        public WikipediaProcessor(IWikiClient client)
        {
            _client = client;
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
                var result = await _client.QueryAsync(query);

                var card = new Card
                {
                    Header = new CardHeader
                    {
                        ImageUrl = result.Thumbnail?.Source,
                        Title = result.Displaytitle,
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
                                        Text = result.ExtractHtml,
                                    }
                                }
                            }
                        }
                    },
                };

                var pagePath = result.ContentUrls?.Desktop?.Page;
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
                                         ChatEventFactory.CreateTextButton("Wikipedia", pagePath)
                                     }
                                 }
                            }
                        });
                }

                return new ChatEventResult(card);
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
