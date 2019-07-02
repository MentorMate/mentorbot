// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;

using MentorBot.Functions.Abstract.Processor;

namespace MentorBot.Functions.Models.TextAnalytics
{
    /// <summary>The result returned from the cognitive service.</summary>
    public class CognitiveTextAnalysisResult
    {
        /// <summary>Initializes a new instance of the <see cref="CognitiveTextAnalysisResult"/> class.</summary>
        public CognitiveTextAnalysisResult(
            TextDeconstructionInformation information,
            ICommandProcessor command,
            IReadOnlyDictionary<string, string> settings)
        {
            TextDeconstructionInformation = information;
            CommandProcessor = command;
            Settings = settings;
        }

        /// <summary>Gets the text deconstruction information.</summary>
        public TextDeconstructionInformation TextDeconstructionInformation { get; }

        /// <summary>Gets most likely command processor.</summary>
        public ICommandProcessor CommandProcessor { get; }

        /// <summary>Gets the settings data.</summary>
        public IReadOnlyDictionary<string, string> Settings { get; }
    }
}
