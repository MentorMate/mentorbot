using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>Gets or sets the answered questions.</summary>
        public List<int> AnsweredQuestions { get; set; }

        /// <summary>Gets or sets a value indicating whether gets or sets the active status.</summary>
        public bool Active { get; set; }
    }
}
