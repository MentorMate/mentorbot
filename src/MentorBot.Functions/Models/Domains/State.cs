using System.Collections.Generic;

using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The user domain model.</summary>
    [Storable("States")]
    public sealed class State
    {
        /// <summary>Gets or sets the email.</summary>
        [RowKey]
        public string UserEmail { get; set; }

        /// <summary>Gets the partition key for this record.</summary>
        [PartitionKey]
        public string PartitionKey { get; } = "System";

        /// <summary>Gets or sets the answered questions.</summary>
        [StoreAsJsonObject]
        public List<string> AnsweredQuestions { get; set; }

        /// <summary>Gets or sets a value indicating whether gets or sets the active status.</summary>
        public bool Active { get; set; }
    }
}
