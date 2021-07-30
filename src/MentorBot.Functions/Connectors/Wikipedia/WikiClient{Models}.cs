// cSpell:ignore pageid
using System.Text.Json.Serialization;

using NS = Newtonsoft.Json;

namespace MentorBot.Functions.Connectors.Wikipedia
{
    /// <summary>The model needed for Wiki client to use.</summary>
    public sealed partial class WikiClient
    {
        /// <summary>The Wikipedia query response.</summary>
        public sealed class QueryResponse
        {
            /// <summary>Gets or sets the type.</summary>
            public string Type { get; set; }

            /// <summary>Gets or sets the title.</summary>
            public string Title { get; set; }

            /// <summary>Gets or sets the display title.</summary>
            public string Displaytitle { get; set; }

            /// <summary>Gets or sets the page identifier.</summary>
            [NS.JsonProperty("pageid")]
            [JsonPropertyName("pageid")]
            public long PageId { get; set; }

            /// <summary>Gets or sets the thumbnail.</summary>
            public Image Thumbnail { get; set; }

            /// <summary>Gets or sets the originalimage.</summary>
            public Image Originalimage { get; set; }

            /// <summary>Gets or sets the language.</summary>
            public string Lang { get; set; }

            /// <summary>Gets or sets the description.</summary>
            public string Description { get; set; }

            /// <summary>Gets or sets the extract text.</summary>
            public string Extract { get; set; }

            /// <summary>Gets or sets the extract HTML.</summary>
            [NS.JsonProperty("extract_html")]
            [JsonPropertyName("extract_html")]
            public string ExtractHtml { get; set; }

            /// <summary>Gets or sets the content urls.</summary>
            [NS.JsonProperty("content_urls")]
            [JsonPropertyName("content_urls")]
            public Content ContentUrls { get; set; }
        }

        /// <summary>The wiki image information.</summary>
        public sealed class Image
        {
            /// <summary>Gets or sets the source url of the image.</summary>
            public string Source { get; set; }

            /// <summary>Gets or sets the image width.</summary>
            public int Width { get; set; }

            /// <summary>Gets or sets the image height.</summary>
            public int Height { get; set; }
        }

        /// <summary>The wiki content information.</summary>
        public sealed class Content
        {
            /// <summary>Gets or sets the web pages information.</summary>
            public Urls Desktop { get; set; }
        }

        /// <summary>The wiki urls information.</summary>
        public sealed class Urls
        {
            /// <summary>Gets or sets the main page url.</summary>
            public string Page { get; set; }
        }
    }
}
