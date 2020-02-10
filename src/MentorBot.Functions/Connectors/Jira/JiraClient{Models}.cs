// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;

namespace MentorBot.Functions.Connectors.Jira
{
    /// <summary>The model needed for Luis client to use.</summary>
    public sealed partial class JiraClient
    {
        /// <summary>The jira response.</summary>
        public sealed class IssuesResponse
        {
            /// <summary>Gets or sets the total.</summary>
            public int Total { get; set; }

            /// <summary>Gets or sets the issues.</summary>
            public IReadOnlyList<Issue> Issues { get; set; }
        }

        /// <summary>The jira issue resource.</summary>
        public sealed class Issue
        {
            /// <summary>Gets or sets the issue identifier.</summary>
            public string Id { get; set; }

            /// <summary>Gets or sets the self url path.</summary>
            public string Self { get; set; }

            /// <summary>Gets or sets the key.</summary>
            public string Key { get; set; }

            /// <summary>Gets or sets the result.</summary>
            public IssueFields Fields { get; set; }
        }

        /// <summary>An issue fields set.</summary>
        public sealed class IssueFields
        {
            /// <summary>Gets or sets the summary.</summary>
            public string Summary { get; set; }

            /// <summary>Gets or sets the assignee.</summary>
            public User Assignee { get; set; }
        }

        /// <summary>The user info.</summary>
        public sealed class User
        {
            /// <summary>Gets or sets the account identifier.</summary>
            public string AccountId { get; set; }

            /// <summary>Gets or sets the key.</summary>
            public string Key { get; set; }

            /// <summary>Gets or sets the email address.</summary>
            public string EmailAddress { get; set; }

            /// <summary>Gets or sets the display name.</summary>
            public string DisplayName { get; set; }
        }
    }
}
