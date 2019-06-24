// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Diagnostics.CodeAnalysis;

namespace MentorBot.Functions.Services.AzureStorage
{
    /// <summary>Azure Storage resource context.</summary>
    [ExcludeFromCodeCoverage]
    public sealed class AzureStorageContext : CoreHelpers.WindowsAzure.Storage.Table.StorageContext, IAzureStorageContext
    {
        /// <summary>Initializes a new instance of the <see cref="AzureStorageContext"/> class.</summary>
        public AzureStorageContext(string connectionString)
            : base(connectionString)
        {
        }
    }
}
