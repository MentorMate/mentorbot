// cSpell:ignore pageid
using System.Text.Json.Serialization;

using NS = Newtonsoft.Json;

namespace MentorBot.Functions.Connectors.Confluence
{
    /// <summary>The model needed for Luis client to use.</summary>
    public sealed partial class ConfluenceClient
    {
        /// <summary>The confluence search responses.</summary>
        public sealed class SearchResponse
        {
            /// <summary>Gets or sets the result.</summary>
            [NS.JsonProperty("results")]
            [JsonPropertyName("results")]
            public Result[] Results { get; set; }

            /// <summary>Gets or sets the extract text.</summary>
            public string Extract { get; set; }

            /// <summary>Gets or sets the extract HTML.</summary>
            [NS.JsonProperty("extract_html")]
            [JsonPropertyName("extract_html")]
            public string ExtractHtml { get; set; }

            /// <summary>Gets or sets the starting page.</summary>
            [NS.JsonProperty("start")]
            [JsonPropertyName("start")]
            public int Start { get; set; }

            /// <summary>Gets or sets the limit.</summary>
            [NS.JsonProperty("limit")]
            [JsonPropertyName("limit")]
            public int Limit { get; set; }

            /// <summary>Gets or sets the size.</summary>
            [NS.JsonProperty("size")]
            [JsonPropertyName("size")]
            public int Size { get; set; }

            /// <summary>Gets or sets the content urls.</summary>
            [NS.JsonProperty("_links")]
            [JsonPropertyName("_links")]
            public Content Links { get; set; }
        }

        /// <summary>The search result information.</summary>
        public sealed class Result
        {
            /// <summary>Gets or sets the page identifier.</summary>
            [NS.JsonProperty("id")]
            [JsonPropertyName("id")]
            public long PageId { get; set; }

            /// <summary>Gets or sets the title.</summary>
            [NS.JsonProperty("type")]
            [JsonPropertyName("type")]
            public string Type { get; set; }

            /// <summary>Gets or sets the status.</summary>
            [NS.JsonProperty("status")]
            [JsonPropertyName("status")]
            public string Status { get; set; }

            /// <summary>Gets or sets the title.</summary>
            [NS.JsonProperty("title")]
            [JsonPropertyName("title")]
            public string Title { get; set; }

            /// <summary>Gets or sets the content urls.</summary>
            [NS.JsonProperty("_links")]
            [JsonPropertyName("_links")]
            public Content Links { get; set; }
        }

        /// <summary>The wiki content information.</summary>
        public sealed class Content
        {
            /// <summary>Gets or sets the web ui.</summary>
            public string WebUi { get; set; }

            /// <summary>Gets or sets the base url.</summary>
            public string Base { get; set; }

            /// <summary>Gets or sets the url context.</summary>
            public string Context { get; set; }

            /// <summary>Gets or sets the next page.</summary>
            public string Next { get; set; }

            /// <summary>Gets or sets the current page.</summary>
            public string Self { get; set; }
        }
    }
}
