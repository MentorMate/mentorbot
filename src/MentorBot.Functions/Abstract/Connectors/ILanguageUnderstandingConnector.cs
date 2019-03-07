// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Abstract.Connectors
{
    /// <summary>A connector that get information from cognitive/language service.</summary>
    public interface ILanguageUnderstandingConnector
    {
        /// <summary>Deconstructs the text asynchronous.</summary>
        /// <param name="text">The text or phrase.</param>
        Task<TextDeconstructionInformation> DeconstructAsync(string text);
    }
}
