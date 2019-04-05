using System;
using System.Collections.Generic;
using System.Text;
using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace MentorBot.Functions.Models.Settings
{
    /// <summary>
    /// A class used to contain all the settings about MentorBot
    /// </summary>
#pragma warning disable CC0021
    [Storable("Settings")]
#pragma warning restore CC0021
    public sealed class MentorBotSettings
    {
        /// <summary>
        /// Gets or sets the key for this record.
        /// </summary>
        [PartitionKey]
        [RowKey]
        public string Key { get; set; } = nameof(MentorBotSettings);

        /// <summary>
        /// Gets or sets the list of all enabled processors
        /// </summary>
        [StoreAsJsonObject]
        public List<string> EnabledProcessors { get; set; }
    }
}
