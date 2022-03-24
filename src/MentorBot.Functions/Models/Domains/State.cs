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
        public List<string> ShownQuestionIds { get; set; } = new List<string>();

        /// <summary>Gets or sets the traits.</summary>
        [StoreAsJsonObject]
        public List<string> Traits { get; set; } = new List<string>();

        /// <summary>Gets or sets the current question's id.</summary>
        public string CurrentQuestionId { get; set; }

        /// <summary>Gets or sets a value indicating whether the status is active.</summary>
        public bool Active { get; set; }
    }
}
