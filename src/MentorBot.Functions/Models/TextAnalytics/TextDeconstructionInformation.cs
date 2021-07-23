using System;
using System.Collections.Generic;

namespace MentorBot.Functions.Models.TextAnalytics
{
    /// <summary>An information related to a sentence or a part of a sentence with meaning on its own.</summary>
    public sealed class TextDeconstructionInformation : IEquatable<TextDeconstructionInformation>
    {
        /// <summary>Initializes a new instance of the <see cref="TextDeconstructionInformation"/> class.</summary>
        public TextDeconstructionInformation(string textSentenceChunk, string subject)
            : this(textSentenceChunk, subject, SentenceTypes.Unknown, new Dictionary<string, string[]>(), null, 1.0)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TextDeconstructionInformation"/> class.</summary>
        public TextDeconstructionInformation(
            string textSentenceChunk,
            string subject,
            SentenceTypes sentenceType,
            IReadOnlyDictionary<string, string[]> entities,
            string[] phrases,
            double confidenceRating)
        {
            TextSentenceChunk = textSentenceChunk;
            Subject = subject;
            SentenceType = sentenceType;
            InformativePhrases = phrases;
            Entities = entities;
            ConfidenceRating = confidenceRating;
        }

        /// <summary>Gets the type of the sentence.</summary>
        public SentenceTypes SentenceType { get; }

        /// <summary>Gets the subject.</summary>
        public string Subject { get; }

        /// <summary>Gets the informative phrases related to the subject.</summary>
        public IReadOnlyList<string> InformativePhrases { get; }

        /// <summary>Gets the informative entities related to the subject.</summary>
        public IReadOnlyDictionary<string, string[]> Entities { get; }

        /// <summary>Gets the text sentence or a chunk of the sentence.</summary>
        public string TextSentenceChunk { get; }

        /// <summary>Gets the confidence rating for selecting the command processor.</summary>
        public double ConfidenceRating { get; }

        /// <inheritdoc/>
        public bool Equals(TextDeconstructionInformation other) =>
            Subject.Equals(other.Subject, StringComparison.InvariantCultureIgnoreCase);

        /// <summary>Equality comparer.</summary>
        public struct EqualityComparer : IEqualityComparer<TextDeconstructionInformation>
        {
            /// <inheritdoc/>
            public bool Equals(TextDeconstructionInformation x, TextDeconstructionInformation y) =>
                x.Equals(y);

            /// <inheritdoc/>
            public int GetHashCode(TextDeconstructionInformation obj) => -1;
        }
    }
}
