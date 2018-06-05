// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Core.Abstract.Processor;

namespace MentorBot.Core.Models.TextAnalytics
{
    /// <summary>The result returned from the cognitive service.</summary>
    public class CognitiveTextAnalysisResult
    {
        /// <summary>Initializes a new instance of the <see cref="CognitiveTextAnalysisResult"/> class.</summary>
        public CognitiveTextAnalysisResult(TextDeconstructionInformation information, ICommandProcessor command, double confidenceRating)
        {
            TextDeconstructionInformation = information;
            CommandProcessor = command;
            ConfidenceRating = confidenceRating;
        }

        /// <summary>Gets the text deconstruction information.</summary>
        public TextDeconstructionInformation TextDeconstructionInformation { get; }

        /// <summary>Gets most likely command processor.</summary>
        public ICommandProcessor CommandProcessor { get; }

        /// <summary>Gets the confidence rating for selecting the command processor.</summary>
        public double ConfidenceRating { get; }
    }
}
