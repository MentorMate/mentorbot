using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>Timesheet processor actions.</summary>
    public interface ITimesheetProcessor : ICommandProcessor
    {
        /// <summary>Gets the timesheets information.</summary>
        /// <param name="dateTime">The current date time.</param>
        /// <param name="state">The state of the timesheet.</param>
        /// <param name="senderEmail">The manager email.</param>
        /// <param name="filterOutSender">Should filter out the sender.</param>
        /// <param name="customersToExclude">The customers to exclude.</param>
        Task<IReadOnlyList<Timesheet>> GetTimesheetsAsync(
            DateTime dateTime,
            TimesheetStates state,
            string senderEmail,
            bool filterOutSender,
            IReadOnlyList<string> customersToExclude);

        /// <summary>Sends the timesheet notifications by user key asynchronous.</summary>
        Task SendTimesheetNotificationsByKeyAsync(
            DateTime date,
            TimesheetStates state,
            string email,
            IReadOnlyList<string> customersToExclude,
            string[] departments,
            bool notify,
            GoogleChatAddress address,
            string userKey,
            Dictionary<string, IReadOnlyList<Timesheet>> timesheets,
            IHangoutsChatConnector connector);
    }
}
