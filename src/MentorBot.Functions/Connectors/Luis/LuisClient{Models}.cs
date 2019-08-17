// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Connectors.Luis
{
    /// <summary>The model needed for Luis client to use.</summary>
    public sealed partial class LuisClient
    {
        /// <summary>The query responses.</summary>
        public sealed class QueryResponse
        {
            /// <summary>Gets or sets the query.</summary>
            public string Query { get; set; }

            /// <summary>Gets or sets the top scoring intent.</summary>
            public ScoringIntent TopScoringIntent { get; set; }

            /// <summary>Gets or sets the intents.</summary>
            public ScoringIntent[] Intents { get; set; }

            /// <summary>Gets or sets the entities.</summary>
            public ScoringEntity[] Entities { get; set; }
        }

        /// <summary>The scoring intent.</summary>
        public sealed class ScoringIntent
        {
            /// <summary>Gets or sets the intent.</summary>
            public string Intent { get; set; }

            /// <summary>Gets or sets the score.</summary>
            public double Score { get; set; }
        }

        /// <summary>The scoring entity.</summary>
        public sealed class ScoringEntity
        {
            /// <summary>Gets or sets the entity.</summary>
            public string Entity { get; set; }

            /// <summary>Gets or sets the type.</summary>
            public string Type { get; set; }

            /// <summary>Gets or sets the start index.</summary>
            public int StartIndex { get; set; }

            /// <summary>Gets or sets the end index.</summary>
            public int EndIndex { get; set; }

            /// <summary>Gets or sets the score.</summary>
            public double Score { get; set; }
        }

        /// <summary>The LUIS utterance info.</summary>
        public sealed class Utterance
        {
            /// <summary>Gets or sets the utterance identifier.</summary>
            public long Id { get; set; }

            /// <summary>Gets or sets the text.</summary>
            public string Text { get; set; }

            /// <summary>Gets or sets the intent label.</summary>
            public string IntentLabel { get; set; }
        }
    }
}
