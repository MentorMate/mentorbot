// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Connectors
{
    /// <summary>Use Microsoft language understanding service to deconstruct a sentance/phrase.</summary>
    public class AzureLuisConnector : ILanguageUnderstandingConnector
    {
        private readonly LuisClient _client;

        /// <summary>Initializes a new instance of the <see cref="AzureLuisConnector"/> class.</summary>
        public AzureLuisConnector(LuisClient client)
        {
            _client = client;
        }

        /// <inheritdoc/>
        public Task<TextDeconstructionInformation> DeconstructAsync(string text) =>
            _client.QueryAsync(text)
                   .ContinueWith(task => CreateInformationIfTopScore(task.Result));

        private static TextDeconstructionInformation CreateInformationIfTopScore(LuisClient.QueryResponse response) =>
            response.TopScoringIntent.Score < 0.75 ? null : CreateInformation(response);

        private static TextDeconstructionInformation CreateInformation(LuisClient.QueryResponse response) =>
            new TextDeconstructionInformation(
                response.Query,
                response.TopScoringIntent.Intent,
                SentenceTypes.Unknown,
                EntitiesToDictoinary(response.Entities),
                null,
                response.TopScoringIntent.Score);

        private static Dictionary<string, string[]> EntitiesToDictoinary(LuisClient.ScoringEntity[] entities) =>
            entities
                .Where(it => it.Score > 0.5)
                .GroupBy(it => it.Type)
                .ToDictionary(it => it.Key, it => it.Select(entity => entity.Entity).ToArray());
    }
}
