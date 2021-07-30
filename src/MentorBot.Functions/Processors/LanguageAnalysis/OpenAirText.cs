// cSpell:ignore unsibmitted, unsubmited, unsibmited, didn, aproved
using System.Collections.Generic;

using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Processors.LanguageAnalysis
{
    /// <summary>Open air text recognition.</summary>
    public static class OpenAirText
    {
        private static readonly IReadOnlyDictionary<string, OpenAirPeriodTypes> Periods = new Dictionary<string, OpenAirPeriodTypes>
        {
            { "last week", OpenAirPeriodTypes.LastWeek },
            { "previous week", OpenAirPeriodTypes.LastWeek },
            { "for the last week", OpenAirPeriodTypes.LastWeek },
            { "this week", OpenAirPeriodTypes.ThisWeek }
        };

        private static readonly IReadOnlyDictionary<string, TimesheetStates> States = new Dictionary<string, TimesheetStates>
        {
            { "unsibmitted", TimesheetStates.Unsubmitted },
            { "unsubmitted", TimesheetStates.Unsubmitted },
            { "unsubmited", TimesheetStates.Unsubmitted },
            { "not unsibmited", TimesheetStates.Unsubmitted },
            { "not unsibmitted", TimesheetStates.Unsubmitted },
            { "not unsubmited", TimesheetStates.Unsubmitted },
            { "not unsubmitted", TimesheetStates.Unsubmitted },
            { "didn ' t submit", TimesheetStates.Unsubmitted },
            { "unapproved", TimesheetStates.Unapproved },
            { "unaproved", TimesheetStates.Unapproved },
            { "not approved", TimesheetStates.Unapproved },
            { "not aproved", TimesheetStates.Unapproved },
            { "non approved", TimesheetStates.Unapproved }
        };

        private static readonly IReadOnlyDictionary<string, string> Phrases = new Dictionary<string, string>
        {
            { "AllAreDoneUnapproved", "<b>All user's timesheets are approved.</b>" },
            { "AllAreDoneUnsubmitted", "<b>All user have have submitted timesheets.</b>" },
            { "SomeAreDoneUnapproved", "The following people have timesheets hours, that are unapproved. <br>" },
            { "SomeAreDoneUnsubmitted", "The following people need to submit timesheets. <br>" },
            { "NotifyUnsubmitted", ", You have unsubmitted timesheet. Please, submit your timesheet." },
            { "NotifyUnapproved", ", Your timesheet is not approved." },
            { "AllAreNotifiedUnsubmitted", "All users with unsubmitted timesheets are notified! Total of {0}." },
            { "AllAreNotifiedUnapproved", "All users with unapproved timesheet are notified! Total of {0}." },
            { "SomeAreNotifiedUnsubmitted", "The following people where not notified and need to submit timesheets. <br>" },
            { "SomeAreNotifiedUnapproved", "The following people where not notified, and have unapproved. <br>" }
        };

        /// <summary>Gets the state of the timesheet.</summary>
        public static TimesheetStates GetTimesheetState(string state) =>
            States.GetValueOrDefault(state ?? string.Empty, TimesheetStates.None);

        /// <summary>Gets the period of the timesheets.</summary>
        public static OpenAirPeriodTypes GetPeriod(string period) =>
            Periods.GetValueOrDefault(period ?? string.Empty, OpenAirPeriodTypes.ThisWeek);

        /// <summary>Gets the open air text.</summary>
        public static string GetText(TimesheetStates state, OpenAirTextTypes type) =>
            Phrases[type.ToString() + state];
    }
}
