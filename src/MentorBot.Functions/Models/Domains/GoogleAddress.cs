// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The google adress metadata.</summary>
    [Storable("Addresses")]
    public sealed class GoogleAddress
    {
        /// <summary>Gets or sets the id of this address.</summary>
        [RowKey]
        public string Id { get; set; } = System.Guid.NewGuid().ToString();

        /// <summary>Gets or sets the PartitionKey .</summary>
        [PartitionKey]
        public string PartitionKey { get; set; }

        /// <summary>Gets or sets the name of the space.</summary>
        [PartitionKey]
        public string SpaceName { get; set; }

        /// <summary>Gets or sets the name of the user.</summary>
        public string UserName { get; set; }

        /// <summary>Gets or sets the email of the user.</summary>
        public string UserEmail { get; set; }

        /// <summary>Gets or sets the display name of the user.</summary>
        public string UserDisplayName { get; set; }
    }
}
