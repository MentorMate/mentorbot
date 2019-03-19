// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.Business
{
    /// <summary>The possible timesheet states.</summary>
    public enum TimesheetStates
    {
        /// <summary>The none selected.</summary>
        None = 0,

        /// <summary>The unsubmitted timesheets.</summary>
        Unsubmitted,

        /// <summary>The unapproved timesheets.</summary>
        Unapproved
    }
}
