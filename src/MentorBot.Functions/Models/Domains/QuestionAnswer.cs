using System.Collections.Generic;

using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary> Either a question or an answer. </summary>
    [Storable("QuestionsAnswers")]
    public sealed class QuestionAnswer
    {
        /// <summary> Gets or sets the id. </summary>
        [RowKey]
        public string Id { get; set; } = System.Guid.NewGuid().ToString();

        /// <summary>Gets the partition key for this record.</summary>
        [PartitionKey]
        public string PartitionKey { get; } = "System";

        /// <summary> Gets or sets the parent id. </summary>
        [StoreAsJsonObject]
        public Dictionary<string, string> Parents { get; set; } = new Dictionary<string, string>();

        /// <summary> Gets or sets the title. </summary>
        public string Title { get; set; }

        /// <summary> Gets or sets the mentormater type. </summary>
        [StoreAsJsonObject]
        public string[] RequiredTraits { get; set; }

        /// <summary> Gets or sets the mentormater type. </summary>
        [StoreAsJsonObject]
        public string[] AcquireTraits { get; set; }

        /// <summary> Gets or sets the content. </summary>
        public string Content { get; set; }

        /// <summary> Gets or sets a value indicating whether the entity is an answer. </summary>
        public bool IsAnswer { get; set; }
    }
}
