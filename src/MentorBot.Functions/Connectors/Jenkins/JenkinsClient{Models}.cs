// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Connectors.Jenkins
{
    /// <summary>The model needed for Luis client to use.</summary>
    public sealed partial class JenkinsClient
    {
        /// <summary>The jenkins job responses.</summary>
        public sealed class JobResponse
        {
            /// <summary>Gets or sets the display name.</summary>
            public string DisplayName { get; set; }

            /// <summary>Gets or sets the description.</summary>
            public string Description { get; set; }

            /// <summary>Gets or sets the URL.</summary>
            public string Url { get; set; }

            /// <summary>Gets or sets the result.</summary>
            public string Result { get; set; }

            /// <summary>Gets or sets a value indicating whether this job is building.</summary>
            public bool Building { get; set; }

            /// <summary>Gets or sets the change set.</summary>
            public ChangeSet ChangeSet { get; set; }
        }

        /// <summary>The source control changeset.</summary>
        public sealed class ChangeSet
        {
            /// <summary>Gets or sets the items.</summary>
            public ChangeSetItem[] Items { get; set; }
        }

        /// <summary>The source control changeset item/commit.</summary>
        public sealed class ChangeSetItem
        {
            /// <summary>Gets or sets the commit comment.</summary>
            public string Comment { get; set; }
        }
    }
}
