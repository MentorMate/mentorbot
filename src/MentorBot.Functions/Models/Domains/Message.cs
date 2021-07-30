using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using MentorBot.Functions.Models.HangoutsChat;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The cognitive service result.</summary>
#pragma warning disable CC0021
    [Storable("Messages")]
#pragma warning restore CC0021
    public sealed class Message
    {
        /// <summary>Gets or sets the Message Id.</summary>
        [RowKey]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the partition key for this record.
        /// </summary>
        [PartitionKey]
        public string PartitionKey { get; set; }

        /// <summary>Gets or sets the user message.</summary>
        public string Input { get; set; }

        /// <summary>Gets or sets the event result.</summary>
        [StoreAsJsonObject]
        public ChatEventResult Output { get; set; }

        /// <summary>Gets or sets the confidence procent from 0 to 100. 0 is not able to tell the comand and 100 is exact match.</summary>
        public int ProbabilityPercentage { get; set; }
    }
}
