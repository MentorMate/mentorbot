// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

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
        /// <summary>Notifies the asynchronous.</summary>
        /// <param name="date">The current date.</param>
        /// <param name="state">The timesheet state.</param>
        /// <param name="email">The requester email.</param>
        /// <param name="customersToExclude">The customers to exclude.</param>
        /// <param name="department">The department.</param>
        /// <param name="notify">if set to <c>true</c> [notify].</param>
        /// <param name="notifyByEmail">if set to <c>true</c> [notify by email].</param>
        /// <param name="filterOutSender">if set to <c>true</c> [do not send norification to sender email].</param>
        /// <param name="address">The chat address.</param>
        /// <param name="connector">The chat connector.</param>
        Task NotifyAsync(
            DateTime date,
            TimesheetStates state,
            string email,
            IReadOnlyList<string> customersToExclude,
            string department,
            bool notify,
            bool notifyByEmail,
            bool filterOutSender,
            GoogleChatAddress address,
            IHangoutsChatConnector connector);

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

        /// <summary>Processes the notify asynchronous.</summary>
        /// <param name="timesheets">The timesheets.</param>
        /// <param name="email">The email.</param>
        /// <param name="department">The department.</param>
        /// <param name="notify">if set to <c>true</c> [notify].</param>
        /// <param name="notifyByEmail">if set to <c>true</c> [notify by email].</param>
        /// <param name="state">The state.</param>
        /// <param name="address">The address.</param>
        /// <param name="connector">The connector.</param>
        Task SendTimesheetNotificationsToUsersAsync(
            IReadOnlyList<Timesheet> timesheets,
            string email,
            string department,
            bool notify,
            bool notifyByEmail,
            TimesheetStates state,
            GoogleChatAddress address,
            IHangoutsChatConnector connector);
    }
}
