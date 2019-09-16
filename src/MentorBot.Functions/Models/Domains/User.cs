// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections;
using System.Collections.Generic;

using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using MentorBot.Functions.Models.Domains.Base;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The user domain model.</summary>
    [Storable("Users")]
    public sealed class User
    {
        /// <summary>Gets or sets the identifier.</summary>
        [RowKey]
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Gets the partition key for this record.</summary>
        [PartitionKey]
        public string PartitionKey { get; } = "System";

        /// <summary>Gets or sets the email.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the open air user identifier.</summary>
        public long OpenAirUserId { get; set; }

        /// <summary>Gets or sets the google account user identifier.</summary>
        public string GoogleUserId { get; set; }

        /// <summary>Gets or sets the user role.</summary>
        public int Role { get; set; }

        /// <summary>Gets or sets the user manager.</summary>
        [StoreAsJsonObject]
        public UserReference Manager { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="User"/> is active.</summary>
        public bool Active { get; set; }

        /// <summary>Gets or sets the department.</summary>
        [StoreAsJsonObject]
        public Department Department { get; set; }

        /// <summary>Gets or sets the customers.</summary>
        [StoreAsJsonObject]
        public Customer[] Customers { get; set; }

        /// <summary>Gets or sets the user properties.</summary>
        [StoreAsJsonObject]
        public IReadOnlyDictionary<string, IReadOnlyList<object>> Properties { get; set; }
    }
}
