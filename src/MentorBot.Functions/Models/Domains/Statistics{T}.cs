using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>A report statistics data.</summary>
    /// <typeparam name="T">The time of the statistics data.</typeparam>
    #pragma warning disable CC0021
    [Storable("Statistics")]
    #pragma warning restore CC0021
    public sealed class Statistics<T>
    {
        /// <summary>Gets or sets the Message Id.</summary>
        [RowKey]
        public string Id { get; set; }

        /// <summary>Gets or sets the partition key for this record. The day of the statistics.</summary>
        [PartitionKey]
        public string Date { get; set; }

        /// <summary>Gets or sets the time.</summary>
        public string Time { get; set; }

        /// <summary>Gets or sets the type.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets the data.</summary>
        [StoreAsJsonObject]
        public T Data { get; set; }
    }
}
