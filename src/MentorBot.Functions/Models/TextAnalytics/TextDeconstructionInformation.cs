// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;

namespace MentorBot.Functions.Models.TextAnalytics
{
    /// <summary>An information related to a sentance or a part of a sentance with meaning on its own.</summary>
    public class TextDeconstructionInformation
    {
        /// <summary>Initializes a new instance of the <see cref="TextDeconstructionInformation"/> class.</summary>
        public TextDeconstructionInformation(string textSentanceChunk, string subject, SentenceTypes sentenceType, params string[] phrases)
        {
            TextSentanceChunk = textSentanceChunk;
            Subject = subject;
            SentenceType = sentenceType;
            InformativePhrases = phrases;
        }

        /// <summary>Gets the type of the sentence.</summary>
        public SentenceTypes SentenceType { get; }

        /// <summary>Gets the subject.</summary>
        public string Subject { get; }

        /// <summary>Gets the informative phrases related to the subject.</summary>
        public IReadOnlyList<string> InformativePhrases { get; }

        /// <summary>Gets the text sentance or a chunk of the sentance.</summary>
        public string TextSentanceChunk { get; }
    }
}
